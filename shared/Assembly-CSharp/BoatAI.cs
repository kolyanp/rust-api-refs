using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ConVar;
using Facepunch;
using Network;
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

		public RaycastHit? hit;
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
				if (((Vector3)(ref worldDirection)).sqrMagnitude > 100f)
				{
					((Vector3)(ref worldDirection)).Normalize();
					boatAI.AddContextInterest(ctx, worldDirection, 0.3f);
				}
			}
			boatAI.AddContextInterest(ctx, val, 0.15f, 2);
			if (_isInDeepsea && (Object)(object)PointEntity<DeepSeaManager>.ServerInstance != (Object)null)
			{
				Vector3 val2 = ((Bounds)(ref DeepSeaManager.DeepSeaBounds)).center - ((Component)boatAI).transform.position;
				val2.y = 0f;
				if (((Vector3)(ref val2)).magnitude > 3000f)
				{
					Vector3 normalized = ((Vector3)(ref val2)).normalized;
					Vector3 worldDirection2 = Vector3.Lerp(val, normalized, 0.75f);
					boatAI.AddContextInterest(ctx, worldDirection2, 0.5f, 0);
				}
			}
			if (boatAI.HasProtectionArea)
			{
				Vector3 val3 = boatAI.ProtectionCenter - ((Component)boatAI).transform.position;
				val3.y = 0f;
				if (((Vector3)(ref val3)).magnitude > 20f)
				{
					Vector3 normalized2 = ((Vector3)(ref val3)).normalized;
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
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			_timeSinceNewTarget = TimeSince.op_Implicit(0f);
			_nextMacroInterval = Random.Range(60f, 150f);
			if (boatAI.HasProtectionArea)
			{
				Vector3 protectionCenter = boatAI.ProtectionCenter;
				_macroTarget = protectionCenter + new Vector3(Random.Range(0f - boatAI.ProtectionRadius, boatAI.ProtectionRadius), 0f, Random.Range(0f - boatAI.ProtectionRadius, boatAI.ProtectionRadius));
				return;
			}
			if ((Object)(object)PointEntity<DeepSeaManager>.ServerInstance == (Object)null)
			{
				_macroTarget = ((Component)boatAI).transform.position + ((Component)boatAI._boat).transform.forward * 100f;
				return;
			}
			Bounds deepSeaBounds = DeepSeaManager.DeepSeaBounds;
			Vector3 min = ((Bounds)(ref deepSeaBounds)).min;
			Vector3 max = ((Bounds)(ref deepSeaBounds)).max;
			_macroTarget = new Vector3(Random.Range(min.x, max.x), ((Component)boatAI).transform.position.y, Random.Range(min.z, max.z));
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
				Vector3 normalized = ((Vector3)(ref val)).normalized;
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
				Vector3 normalized = ((Vector3)(ref val)).normalized;
				Vector3 val2 = position2 + normalized * 15f;
				Vector3 val3 = Vector3.Cross(Vector3.up, normalized);
				Vector3 normalized2 = ((Vector3)(ref val3)).normalized;
				Vector3 val4 = ((Vector3.Dot(((Component)boatAI._boat).transform.forward, normalized2) > 0f) ? normalized2 : (-normalized2)) * 25f;
				Vector3 val5 = val2 + val4;
				Vector3 val6 = val5 - ((Component)boatAI._boat).transform.position;
				Vector3 normalized3 = ((Vector3)(ref val6)).normalized;
				normalized3.y = 0f;
				boatAI.AddContextInterest(ctx, normalized3, 1f, 2);
				float num = Vector3Ex.Distance2D(((Component)boatAI._boat).transform.position, val5);
				if (_beenClose)
				{
					if (Vector3.Dot(((Component)boatAI._boat).transform.forward, ((Vector3)(ref val)).normalized) < 0f)
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
			if (playerTarget == null || flag)
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
				Vector3 normalized = ((Vector3)(ref val)).normalized;
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
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)boatAI._boat == (Object)null)
			{
				return;
			}
			if (!boatAI.HasProtectionArea)
			{
				boatAI.SwitchState(boatAI._wanderState);
			}
			if (Vector3.Distance(((Component)boatAI).transform.position, currentOrbitTargetPoint) < 60f)
			{
				orbitPercent += 1f / 45f;
				if (orbitPercent > 1f)
				{
					orbitPercent -= 1f;
				}
				float radAngle = orbitPercent * (MathF.PI * 2f);
				currentOrbitTargetPoint = GetPointOnCircle(boatAI.ProtectionCenter, 80f, radAngle);
			}
			Vector3 val = currentOrbitTargetPoint - ((Component)boatAI).transform.position;
			Vector3 normalized = ((Vector3)(ref val)).normalized;
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

	private FeelerResult[] feelers = new FeelerResult[8];

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

	private const float GROUP_COHESION_RADIUS = 30f;

	private const float GROUP_PULL_STRENGTH = 0.25f;

	private const int CONTEXT_RESOLUTION = 8;

	[SerializeField]
	[Header("Boat AI - Scientists")]
	private bool _autoFillWithScientists;

	[SerializeField]
	private GameObjectRef _scientistPrefab;

	private HumanNPC _driverNpc;

	private Dictionary<HumanNPC, MountedWeaponSeat> _turretNpcs = new Dictionary<HumanNPC, MountedWeaponSeat>();

	private bool _hasSpawnedScientists;

	private bool _hasKilledScientists;

	private List<AiMountedWeaponController> _mountedWeaponControllers;

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

	[Header("Boat AI - Collision Avoidance")]
	[SerializeField]
	private float _awarenessAngle;

	[SerializeField]
	private float _awarenessDistance;

	[Header("Boat AI - Debug")]
	[SerializeField]
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

	private int _driveDirection = 1;

	private float _driveLockTimer;

	private float _stuckTimer;

	private Vector3 _lastPos;

	[ServerVar(Help = "(Generated) When enabled, boat AI entities can enter a sleep state when no players are nearby; reduce CPU usage for idle boats")]
	public static bool allow_sleeping = false;

	[ServerVar(Help = "(Generated) Number of seconds a boat AI will wait without player interaction before entering sleep mode; default 30s")]
	public static float seconds_until_sleep = 30f;

	private const float AI_SPAWN_DELAY = 5f;

	private InputProvider _provider;

	private TimeSince _timeSinceThought;

	private TimeSince _timeSinceSleepy;

	private TimeSince _timeSinceSpawned;

	private bool _isSleeping;

	private bool _setupRan;

	private ScientistBoatOilrigManager _oilrigManager;

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

	public Vector3 ProtectionCenter { get; set; }

	public float ProtectionRadius { get; set; }

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
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)_boat).transform.position;
		QueryParameters val = default(QueryParameters);
		((QueryParameters)(ref val))._002Ector(1218781441, false, (QueryTriggerInteraction)0, false);
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
			RaycastHit? hit = null;
			RaycastHit value;
			for (int j = 0; j < 8; j++)
			{
				RaycastHit val = hits[num + j];
				if ((Object)(object)((RaycastHit)(ref val)).collider == (Object)null || (ColliderEx.IsOnLayer(((RaycastHit)(ref val)).collider, (Layer)18) && !((Component)((RaycastHit)(ref val)).collider).CompareTag("BoatAIAvoid")))
				{
					continue;
				}
				BaseEntity entity = RaycastHitEx.GetEntity(val);
				if ((Object)(object)entity == (Object)(object)this || (Object)(object)((Component)((RaycastHit)(ref val)).collider).transform.root == (Object)(object)((Component)_boat).transform || (Check.EntityValid(entity) && Check.EntityIsClient(entity)))
				{
					continue;
				}
				if (hit.HasValue)
				{
					float distance = ((RaycastHit)(ref val)).distance;
					value = hit.Value;
					if (!(distance < ((RaycastHit)(ref value)).distance))
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
				if (!(((RaycastHit)(ref value)).distance < _closestObstacle.Value))
				{
					continue;
				}
			}
			value = hit.Value;
			_closestObstacle = ((RaycastHit)(ref value)).distance;
		}
	}

	private void AvoidObstacles(Context ctx)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
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
				NativeArray<RaycastCommand> val = default(NativeArray<RaycastCommand>);
				val._002Ector(count, (Allocator)3, (NativeArrayOptions)1);
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
			FeelerResult[] array = feelers;
			for (int i = 0; i < array.Length; i++)
			{
				FeelerResult feelerResult = array[i];
				if (feelerResult.hit.HasValue)
				{
					RaycastHit value = feelerResult.hit.Value;
					float strength = Mathf.Clamp01(1f - ((RaycastHit)(ref value)).distance / _awarenessDistance);
					AddContextDanger(ctx, feelerResult.direction, strength, 0);
				}
			}
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
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		if (_contextMap.Count != 8)
		{
			_contextMap.Clear();
			float num = 45f;
			for (int i = 0; i < 8; i++)
			{
				Vector3 item = Quaternion.AngleAxis((float)i * num, Vector3.up) * Vector3.forward;
				_contextMap.Add(item);
			}
		}
		_bufferContext = new Context(8);
	}

	private int GetContextIndex(Vector3 worldDirection)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		Vector3 normalized = ((Vector3)(ref worldDirection)).normalized;
		normalized.y = 0f;
		int result = 0;
		float num = float.NegativeInfinity;
		for (int i = 0; i < _contextMap.Count; i++)
		{
			float num2 = Vector3.Dot(normalized, _contextMap[i]);
			if (num2 > num)
			{
				num = num2;
				result = i;
			}
		}
		return result;
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
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("BoatAI.Context.GetBestContextDirection"))
		{
			if (_contextMap.Count == 0)
			{
				return ((Component)this).transform.forward;
			}
			BlurContext(_bufferContext);
			int num = -1;
			float num2 = float.MinValue;
			for (int i = 0; i < _bufferContext.InterestMap.Length; i++)
			{
				float num3 = _bufferContext.InterestMap[i];
				float num4 = _bufferContext.DangerMap[i];
				float num5 = num3 - num4;
				if ((double)num5 > (double)num2 + 0.05)
				{
					num2 = num5;
					num = i;
				}
			}
			if (num == -1 || num2 <= 0.01f)
			{
				return Vector3.zero;
			}
			_ = _lastBestIndex;
			_lastBestIndex = num;
			return _contextMap[num];
		}
	}

	private void AddContextInterest(Context context, Vector3 worldDirection, float strength, int spread = 1)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		int contextIndex = GetContextIndex(worldDirection);
		for (int i = -spread; i <= spread; i++)
		{
			int num = (contextIndex + i + context.InterestMap.Length) % context.InterestMap.Length;
			if (num >= 0 && num < context.InterestMap.Length)
			{
				float num2 = 1f - (float)Mathf.Abs(i) / (float)(spread + 1);
				float num3 = strength * num2;
				context.InterestMap[num] += num3;
			}
		}
	}

	private void AddContextDanger(Context context, Vector3 worldDirection, float strength, int spread = 1)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		int contextIndex = GetContextIndex(worldDirection);
		for (int i = -spread; i <= spread; i++)
		{
			int num = (contextIndex + i + context.DangerMap.Length) % context.DangerMap.Length;
			if (num >= 0 && num < context.DangerMap.Length)
			{
				float num2 = 1f - (float)Mathf.Abs(i) / (float)(spread + 1);
				float num3 = strength * num2;
				context.DangerMap[num] += num3;
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
			if (mountable is RHIBDriver || mountable is MountedWeaponSeat)
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
		_mountedWeaponControllers = new List<AiMountedWeaponController>();
		_mountedWeaponControllers = (from seat in _turretNpcs.Values
			select seat.MountedWeaponGameObject.GetComponent<AiMountedWeaponController>() into controller
			where (Object)(object)controller != (Object)null
			select controller).ToList();
	}

	public void GiveMountedWeaponsExtraTarget(BasePlayer ply)
	{
		if (_mountedWeaponControllers == null)
		{
			return;
		}
		foreach (AiMountedWeaponController mountedWeaponController in _mountedWeaponControllers)
		{
			mountedWeaponController.SetExtraTarget(ply);
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
			if (allMountPoint.mountable is MountedWeaponSeat)
			{
				mounted.inventory.containerBelt.Clear();
				mounted.inventory.containerMain.Clear();
				_turretNpcs.Add(mounted as HumanNPC, allMountPoint.mountable as MountedWeaponSeat);
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
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (BaseNetworkableEx.Is<HumanNPC>((Object)(object)_scientistPrefab.GetEntity(), out HumanNPC _))
		{
			BaseEntity baseEntity = GameManager.server.CreateEntity(_scientistPrefab.resourcePath, _boat.mountAnchor.position, Quaternion.identity);
			baseEntity.Spawn();
			HumanNPC humanNPC = baseEntity as HumanNPC;
			mountable.AttemptMount(humanNPC, doMountChecks: false);
			if ((Object)(object)humanNPC.Brain != (Object)null && humanNPC.Brain.Senses != null)
			{
				humanNPC.Brain.Senses.ignoreTutorialPlayers = true;
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

	private float GetSteerToTarget(Vector3 targetPosition)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Component)_boat).transform.InverseTransformPoint(targetPosition);
		float num = Mathf.Clamp(0f - val.x, -1f, 1f);
		if (val.z < 0f)
		{
			num = ((num >= 0f) ? 1f : (-1f));
		}
		return Mathf.Clamp(num, -1f, 1f);
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
				Vector3 normalized = ((Vector3)(ref val2)).normalized;
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
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
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
		if (!Check.IsValidAttackTarget(ply))
		{
			if (PRINT_DEBUGS)
			{
				Debug.LogWarning((object)("[BoatAI] Invalid target: " + ply.displayName + " not valid attack target"));
			}
			return false;
		}
		if (Check.SimplyOnTerrainAt(((Component)ply).transform.position))
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
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		return GamePhysics.LineOfSight(((Component)_boat).transform.position, ply.eyes.position, 153092352);
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
			Vector3[] obj = new Vector3[3]
			{
				val3 * 10f,
				-val3 * 5f - val4 * 7.5f,
				-val3 * 5f + val4 * 7.5f
			};
			int nextGroupId = BoatAICoordination.GetNextGroupId();
			bool flag = false;
			Vector3[] array = (Vector3[])(object)obj;
			foreach (Vector3 val5 in array)
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
		position = default(Vector2);
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

	private void SetupAI(BaseBoat boat)
	{
		if (!((Object)(object)boat == (Object)null) && !_setupRan)
		{
			Invoke(delegate
			{
				SetupInternal(boat);
			}, 1f);
		}
	}

	private void SetupInternal(BaseBoat boat)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		_setupRan = true;
		_boat = boat;
		Boat = boat as MotorRowboat;
		_lastPos = ((Component)_boat).transform.position;
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
		SwitchState(_idleState);
		RefreshSleeping();
	}

	private void TargetCheck()
	{
		using (TimeWarning.New("BoatAi.TargetCheck"))
		{
			if (ActiveTarget != null && !ActiveTarget.IsValid(this))
			{
				ClearCurrentTarget();
			}
		}
	}

	public override void Save(SaveInfo info)
	{
		if (!base.isClient && (!info.forDisk || LoadMode != AILoadMode.KillAi))
		{
			base.Save(info);
		}
	}

	public void OnTargetClaimAvailable(BasePlayer ply)
	{
		if (Check.EntityValid(ply) && ActiveTarget is PlayerTarget playerTarget && ply.userID.Get() == playerTarget.Player.userID.Get())
		{
			PursuePlayer(ply);
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

	public void SetOilRigManager(ScientistBoatOilrigManager manager)
	{
		_oilrigManager = manager;
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
			BoatAICoordination.Unregister(this);
			if (InGroup)
			{
				BoatAICoordination.RemoveFromGroup(this, GroupId);
			}
			if ((Object)(object)_oilrigManager != (Object)null)
			{
				_oilrigManager.AIDestroyed(_boat as RHIB);
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
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_031c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0333: Unknown result type (might be due to invalid IL or missing references)
		//IL_0338: Unknown result type (might be due to invalid IL or missing references)
		//IL_0342: Unknown result type (might be due to invalid IL or missing references)
		//IL_0347: Unknown result type (might be due to invalid IL or missing references)
		//IL_035e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0375: Unknown result type (might be due to invalid IL or missing references)
		//IL_037a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0384: Unknown result type (might be due to invalid IL or missing references)
		//IL_0389: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03da: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0406: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_05bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_05cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_05fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0606: Unknown result type (might be due to invalid IL or missing references)
		//IL_0442: Unknown result type (might be due to invalid IL or missing references)
		//IL_0452: Unknown result type (might be due to invalid IL or missing references)
		//IL_0457: Unknown result type (might be due to invalid IL or missing references)
		//IL_045c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0460: Unknown result type (might be due to invalid IL or missing references)
		//IL_0465: Unknown result type (might be due to invalid IL or missing references)
		//IL_047e: Unknown result type (might be due to invalid IL or missing references)
		//IL_048e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0493: Unknown result type (might be due to invalid IL or missing references)
		//IL_049a: Unknown result type (might be due to invalid IL or missing references)
		//IL_049f: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_064a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0656: Unknown result type (might be due to invalid IL or missing references)
		//IL_0660: Unknown result type (might be due to invalid IL or missing references)
		//IL_0665: Unknown result type (might be due to invalid IL or missing references)
		//IL_0693: Unknown result type (might be due to invalid IL or missing references)
		//IL_0585: Unknown result type (might be due to invalid IL or missing references)
		//IL_058f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0536: Unknown result type (might be due to invalid IL or missing references)
		//IL_053b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0545: Unknown result type (might be due to invalid IL or missing references)
		//IL_054a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0565: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_boat == (Object)null)
		{
			return;
		}
		using (TimeWarning.New("BoatAI.OnTick"))
		{
			if (IsScientistDriverDead())
			{
				RemoveAI();
				return;
			}
			gasPedal = 0f;
			if (TimeSince.op_Implicit(_timeSinceSpawned) < 5f)
			{
				return;
			}
			if ((_isSleeping || !BaseNetworkable.HasCloseConnections(((Component)this).transform.position, enable_mount_sync_distance)) && !HasValidPursuit())
			{
				_boat.DisableMountedSyncForAllSeats();
				return;
			}
			_boat.EnableMountedSyncForAllSeats();
			if (!AI.move)
			{
				return;
			}
			ResetContext(_bufferContext);
			RunGlobalMethods();
			if (!Boat.IsOn())
			{
				StartEngine(Boat);
			}
			using (TimeWarning.New("BoatAI.OnTick.CurrentState.Update"))
			{
				_currentState?.Update(_bufferContext, this, delta);
				Vector3 bestContextDirection = GetBestContextDirection();
				Vector3 targetPosition = ((Component)_boat).transform.position + ((Vector3)(ref bestContextDirection)).normalized * 10f;
				float steerToTarget = GetSteerToTarget(targetPosition);
				steering = steerToTarget;
				float num = Vector3.Dot(((Component)_boat).transform.forward, ((Vector3)(ref bestContextDirection)).normalized);
				float magnitude = ((Vector3)(ref bestContextDirection)).magnitude;
				Vector3 position = ((Component)_boat).transform.position;
				Vector3 val = position - _lastPos;
				_lastPos = position;
				val.y = 0f;
				float num2 = ((Vector3)(ref val)).magnitude / Mathf.Max(delta, 0.001f);
				if (((Vector3)(ref bestContextDirection)).sqrMagnitude > Mathf.Epsilon && num2 < 0.6f)
				{
					_stuckTimer += delta;
				}
				else
				{
					_stuckTimer = 0f;
				}
				bool flag = _stuckTimer >= 1f;
				if (_driveLockTimer <= 0f)
				{
					if (_driveDirection == 1 && flag && num < -0.4f)
					{
						_driveDirection = -1;
						_driveLockTimer = 0.4f;
					}
					else if (_driveDirection == -1 && num > -0.15f)
					{
						_driveDirection = 1;
						_driveLockTimer = 0.4f;
					}
				}
				else
				{
					_driveLockTimer -= delta;
				}
				if (bestContextDirection == Vector3.zero)
				{
					gasPedal = 0f;
				}
				else
				{
					float num3 = magnitude * (float)_driveDirection;
					gasPedal = Mathf.Clamp(num3, 0f - max_speed_percentage, max_speed_percentage);
				}
				if (_driveDirection < 0)
				{
					steering *= -1f;
				}
				if (ActiveTarget != null && Vector3Ex.Distance2D(((Component)_boat).transform.position, ActiveTarget.Position.Value) < 5f)
				{
					gasPedal = 0f;
				}
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
					Vector3 val2 = ActiveTarget.Position.Value - ((Component)_boat).transform.position;
					Vector3 normalized = ((Vector3)(ref val2)).normalized;
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
					float num4 = _bufferContext.DangerMap[i];
					float num5 = _bufferContext.InterestMap[i];
					UnityEngine.DDraw.BroadcastText(((Component)_boat).transform.position + _contextMap[i] * 5f, string.Format("INDEX {0}, Dgr {1}, Int {2}", i, num4.ToString("F2"), num5.ToString("F2")), Color.black, 0.05f, distanceFade: false);
				}
			}
		}
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
			RunThink();
		}
	}

	private void RunThink()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (TimeSince.op_Implicit(_timeSinceThought) < _thinkTime)
		{
			return;
		}
		using (TimeWarning.New("BoatAI.RunThink"))
		{
			_timeSinceThought = TimeSince.op_Implicit(0f);
			if (_autoPursue && (!HasValidPursuit() || !(ActiveTarget is PlayerTarget)))
			{
				BasePlayer basePlayer = FindClosestPlayerTarget();
				if ((Object)(object)basePlayer != (Object)null)
				{
					PursuePlayer(basePlayer);
				}
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

	private void PursuePlayer(BasePlayer ply, bool applyToGroup = true)
	{
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)ply == (Object)null)
		{
			if (PRINT_DEBUGS)
			{
				Debug.LogWarning((object)(((Object)this).name + ": PursuePlayer called with null player"));
			}
			return;
		}
		if ((Object)(object)_boat == (Object)null)
		{
			if (PRINT_DEBUGS)
			{
				Debug.LogWarning((object)(((Object)this).name + ": PursuePlayer called before Setup() \ufffd _boat is null"));
			}
			return;
		}
		if (_seekState == null)
		{
			if (PRINT_DEBUGS)
			{
				Debug.LogWarning((object)(((Object)this).name + ": PursuePlayer called before SetupStateCache()"));
			}
			return;
		}
		if (!IsPlayerTargetValid(ply))
		{
			if (PRINT_DEBUGS)
			{
				Debug.Log((object)"Player isnt valid");
			}
			return;
		}
		if (!IsPlayerInRange(ply, _searchRange))
		{
			if (PRINT_DEBUGS)
			{
				Debug.Log((object)"Player isnt in range");
			}
			return;
		}
		if (ActiveTarget is PlayerTarget playerTarget)
		{
			BoatAICoordination.ReleaseClaim(this, playerTarget.Player);
		}
		bool stayClose = BoatAICoordination.TryClaimTarget(this, ply);
		PlayerTarget moveCommand = new PlayerTarget(ply, Time.time, ((Component)_boat).transform)
		{
			StayClose = stayClose
		};
		SetMoveCommand(moveCommand);
		PursuitTargetAcquireTime = Time.time;
		if (!(InGroup && applyToGroup))
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
				if (!((Object)(object)current == (Object)null) && !((Object)(object)current == (Object)(object)this))
				{
					current.PursuePlayer(ply, applyToGroup: false);
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	private BasePlayer FindClosestPlayerTarget()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
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
					if (IsPlayerTargetValid(item) && IsPlayerInRange(item, _searchRange) && !(((Component)item).transform.position.y < -5f) && !BoatAICoordination.IsTargetClaimedByAnotherGroup(this, item))
					{
						float num2 = Vector3.SqrMagnitude(((Component)item).transform.position - ((Component)_boat).transform.position);
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
		if ((Object)(object)info.InitiatorPlayer != (Object)null)
		{
			BasePlayer initiatorPlayer = info.InitiatorPlayer;
			if (IsPlayerTargetValid(initiatorPlayer))
			{
				PursuePlayer(info.InitiatorPlayer);
			}
		}
	}

	private void BoatDied()
	{
		if (_hasSpawnedScientists)
		{
			KillAllRemainingScientists();
		}
	}

	protected unsafe override bool WriteSyncVar(byte id, NetWrite writer)
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
				Debug.Log((object)("SyncVar Writing: GroupId for " + ((object)(*(NetworkableId*)(&iD))/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_GroupId);
			return true;
		case 1:
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: BoatID for " + ((object)(*(NetworkableId*)(&iD))/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite<NetworkableId>(writer, __sync_BoatID);
			return true;
		case 2:
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: LoadModeSync for " + ((object)(*(NetworkableId*)(&iD))/*cast due to constrained. prefix*/).ToString()));
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
			"GroupId" => 0, 
			"BoatID" => 1, 
			"LoadModeSync" => 2, 
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
		var (src, num) = netWrite.GetBuffer();
		if (_autosaveBuffer == null)
		{
			_autosaveBuffer = BaseEntity._autosaveBufferPool.Rent(num);
		}
		if (_autosaveBuffer.Length < num)
		{
			BaseEntity._autosaveBufferPool.Return(_autosaveBuffer);
			_autosaveBuffer = BaseEntity._autosaveBufferPool.Rent(num);
		}
		Buffer.BlockCopy(src, 0, _autosaveBuffer, 0, num);
		save.msg.baseEntity.syncVars = _autosaveBuffer;
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
		__sync_BoatID = default(NetworkableId);
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

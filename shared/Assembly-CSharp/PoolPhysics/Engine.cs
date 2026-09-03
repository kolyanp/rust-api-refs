using System;
using System.Collections.Generic;
using Facepunch;
using UnityEngine;

namespace PoolPhysics;

public class Engine : IPooled
{
	public struct RaycastHit
	{
		public Vector2 Point;

		public Vector2 Normal;

		public float Distance;

		public int BallId;

		public int WallIndex;
	}

	public Action<int> OnBallPocketed;

	public Action<Vector2, float> OnBallCollision;

	public Action<Vector2, float> OnWallCollision;

	public float DragConstant = 0.55f;

	private BufferList<Data.Ball> balls;

	private BufferList<Data.Wall> walls;

	private BufferList<Data.Pocket> pockets;

	private Dictionary<int, int> ballIndexLookup;

	private bool hasInitialised;

	public const float StopThresholdSqr = 0.005f;

	private const float MaxSubstepTravelFraction = 0.5f;

	private const int MaxSubsteps = 32;

	public BufferList<Data.Ball> Balls => balls;

	public BufferList<Data.Wall> Walls => walls;

	public BufferList<Data.Pocket> Pockets => pockets;

	public bool IsReady => hasInitialised;

	public Engine()
	{
		Setup();
	}

	~Engine()
	{
		Teardown();
	}

	private void Setup()
	{
		if (balls == null)
		{
			balls = Pool.Get<BufferList<Data.Ball>>();
		}
		if (walls == null)
		{
			walls = Pool.Get<BufferList<Data.Wall>>();
		}
		if (pockets == null)
		{
			pockets = Pool.Get<BufferList<Data.Pocket>>();
		}
		ballIndexLookup = Pool.Get<Dictionary<int, int>>();
		hasInitialised = true;
	}

	private void Teardown()
	{
		if (hasInitialised)
		{
			if (balls != null)
			{
				balls.Clear();
			}
			if (walls != null)
			{
				walls.Clear();
			}
			if (pockets != null)
			{
				pockets.Clear();
			}
			if (ballIndexLookup != null)
			{
				ballIndexLookup.Clear();
			}
			if (balls != null)
			{
				Pool.FreeUnmanaged<Data.Ball>(ref balls);
			}
			if (walls != null)
			{
				Pool.FreeUnmanaged<Data.Wall>(ref walls);
			}
			if (pockets != null)
			{
				Pool.FreeUnmanaged<Data.Pocket>(ref pockets);
			}
			if (ballIndexLookup != null)
			{
				Pool.FreeUnmanaged<int, int>(ref ballIndexLookup);
			}
		}
	}

	public void EnterPool()
	{
		Teardown();
	}

	public void LeavePool()
	{
		Setup();
	}

	public void AddBall(Data.Ball ball)
	{
		if (hasInitialised && !ballIndexLookup.ContainsKey(ball.Id))
		{
			ballIndexLookup[ball.Id] = balls.Count;
			balls.Add(ball);
		}
	}

	public void AddWall(Data.Wall wall)
	{
		if (hasInitialised)
		{
			walls.Add(wall);
		}
	}

	public void AddPocket(Data.Pocket pocket)
	{
		if (hasInitialised)
		{
			pockets.Add(pocket);
		}
	}

	public void RemoveBall(int id)
	{
		if (hasInitialised)
		{
			if (!ballIndexLookup.TryGetValue(id, out var value))
			{
				Log("Can't get ball with id " + id + " - not found");
				return;
			}
			balls.RemoveAt(value);
			ballIndexLookup.Remove(id);
			RebuildBallIndexLookup();
		}
	}

	public Data.Ball GetBall(int id)
	{
		if (!hasInitialised)
		{
			return default(Data.Ball);
		}
		if (!ballIndexLookup.TryGetValue(id, out var value))
		{
			Log("Can't get ball with id " + id + " - not found");
			return default(Data.Ball);
		}
		return balls[value];
	}

	public void SetBallPosition(int id, Vector2 pos)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (hasInitialised && ballIndexLookup.TryGetValue(id, out var value))
		{
			Data.Ball ball = balls[value];
			ball.Position = pos;
			ball.Velocity = Vector2.zero;
			balls[value] = ball;
		}
	}

	public void SetBallVelocity(int id, Vector2 vel)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (hasInitialised && ballIndexLookup.TryGetValue(id, out var value))
		{
			Data.Ball ball = balls[value];
			ball.Velocity = vel;
			balls[value] = ball;
		}
	}

	public void SetBallIsKinematic(int id, bool isKinematic)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		if (hasInitialised && ballIndexLookup.TryGetValue(id, out var value))
		{
			Data.Ball ball = balls[value];
			ball.IsKinematic = isKinematic;
			if (isKinematic)
			{
				ball.Velocity = Vector2.zero;
			}
			balls[value] = ball;
		}
	}

	public void ApplyForce(int id, Vector2 force)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if (hasInitialised && ballIndexLookup.TryGetValue(id, out var value))
		{
			Data.Ball ball = balls[value];
			ref Vector2 velocity = ref ball.Velocity;
			velocity += force;
			balls[value] = ball;
		}
	}

	public bool HasMovingBalls()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<Data.Ball> enumerator = balls.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				Data.Ball current = enumerator.Current;
				if (!current.IsKinematic && ((Vector2)(ref current.Velocity)).sqrMagnitude > 0.005f)
				{
					return true;
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		return false;
	}

	public bool Raycast(Vector2 origin, Vector2 direction, float maxDistance, out RaycastHit hit, int ignoreBallId = -1, bool includeBalls = true, bool includeWalls = true)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		hit = default(RaycastHit);
		if (!hasInitialised)
		{
			return false;
		}
		if (maxDistance <= 0f)
		{
			return false;
		}
		if (((Vector2)(ref direction)).sqrMagnitude <= Mathf.Epsilon)
		{
			return false;
		}
		((Vector2)(ref direction)).Normalize();
		bool result = false;
		float num = maxDistance;
		if (includeWalls)
		{
			for (int i = 0; i < walls.Count; i++)
			{
				if (IntersectRaySegment(origin, direction, walls[i].A, walls[i].B, out var distance) && !(distance < 0f) && !(distance > num))
				{
					num = distance;
					result = true;
					hit.Point = origin + direction * distance;
					hit.Normal = walls[i].Normal;
					hit.Distance = distance;
					hit.BallId = -1;
					hit.WallIndex = i;
				}
			}
		}
		if (includeBalls)
		{
			for (int j = 0; j < balls.Count; j++)
			{
				Data.Ball ball = balls[j];
				if (!ball.IsKinematic && ball.Id != ignoreBallId && IntersectRayCircle(origin, direction, ball.Position, ball.Radius, out var distance2) && !(distance2 < 0f) && !(distance2 > num))
				{
					Vector2 val = origin + direction * distance2;
					Vector2 val2 = val - ball.Position;
					Vector2 normalized = ((Vector2)(ref val2)).normalized;
					num = distance2;
					result = true;
					hit.Point = val;
					hit.Normal = normalized;
					hit.Distance = distance2;
					hit.BallId = ball.Id;
					hit.WallIndex = -1;
				}
			}
		}
		return result;
	}

	public void Tick(float delta)
	{
		if (!hasInitialised || balls.Count == 0)
		{
			return;
		}
		using (TimeWarning.New("PoolPhysics.Engine.Tick"))
		{
			int num = ComputeSubsteps(delta);
			float delta2 = delta / (float)num;
			for (int i = 0; i < num; i++)
			{
				IntegratePositions(delta2);
				ResolveWallCollisions();
				ResolveBallCollisions();
				ResolvePockets();
			}
			ApplyDrag(delta);
		}
	}

	private int ComputeSubsteps(float delta)
	{
		using (TimeWarning.New("PoolPhysics.Engine.Tick.ComputeSubsteps"))
		{
			float num = 0f;
			float num2 = float.MaxValue;
			for (int i = 0; i < balls.Count; i++)
			{
				if (!balls[i].IsKinematic)
				{
					Data.Ball ball = balls[i];
					float sqrMagnitude = ((Vector2)(ref ball.Velocity)).sqrMagnitude;
					if (sqrMagnitude > num)
					{
						num = sqrMagnitude;
					}
					if (balls[i].Radius < num2)
					{
						num2 = balls[i].Radius;
					}
				}
			}
			if (num <= 0f || num2 == float.MaxValue || num2 <= 0f)
			{
				return 1;
			}
			float num3 = Mathf.Sqrt(num) * delta;
			float num4 = num2 * 0.5f;
			return Mathf.Clamp(Mathf.CeilToInt(num3 / num4), 1, 32);
		}
	}

	private void IntegratePositions(float delta)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("PoolPhysics.Engine.Tick.IntegratePositions"))
		{
			for (int i = 0; i < balls.Count; i++)
			{
				Data.Ball ball = balls[i];
				if (!ball.IsKinematic)
				{
					ref Vector2 position = ref ball.Position;
					position += ball.Velocity * delta;
					balls[i] = ball;
				}
			}
		}
	}

	private void ResolveWallCollisions()
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("PoolPhysics.Engine.Tick.ResolveWallCollisions"))
		{
			if (!hasInitialised || walls.Count == 0)
			{
				return;
			}
			for (int i = 0; i < balls.Count; i++)
			{
				Data.Ball ball = balls[i];
				if (ball.IsKinematic)
				{
					continue;
				}
				for (int j = 0; j < walls.Count; j++)
				{
					Data.Wall wall = walls[j];
					Vector2 val = ClosestPointOnSegment(Balls[i].Position, Walls[j].A, Walls[j].B);
					Vector2 val2 = Balls[i].Position - val;
					if (((Vector2)(ref val2)).magnitude < ball.Radius)
					{
						float num = Vector2.Dot(ball.Position - val, wall.Normal);
						ref Vector2 position = ref ball.Position;
						position += wall.Normal * (ball.Radius - num);
						if (Vector2.Dot(ball.Velocity, wall.Normal) < 0f)
						{
							ball.Velocity = Vector2.Reflect(ball.Velocity, wall.Normal);
							ref Vector2 velocity = ref ball.Velocity;
							velocity *= 0.85f;
							float arg = Vector2.Dot(ball.Velocity, wall.Normal);
							OnWallCollision?.Invoke(ball.Position, arg);
						}
						balls[i] = ball;
					}
				}
			}
		}
	}

	private void ApplyDrag(float delta)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("PoolPhysics.Engine.Tick.ApplyDrag"))
		{
			if (!hasInitialised)
			{
				return;
			}
			float num = DeterministicExp((0f - DragConstant) * delta);
			Enumerator<Data.Ball> enumerator = balls.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Data.Ball current = enumerator.Current;
					if (!current.IsKinematic)
					{
						Vector2 val = current.Velocity * num;
						SetBallVelocity(current.Id, (((Vector2)(ref val)).sqrMagnitude > 0.005f) ? val : Vector2.zero);
					}
				}
			}
			finally
			{
				((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
			}
		}
	}

	private static float DeterministicExp(float x)
	{
		return 1f + x * (1f + x * (0.5f + x * (1f / 6f + x * (1f / 24f))));
	}

	private void ResolveBallCollisions()
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		if (!hasInitialised || balls.Count == 0)
		{
			return;
		}
		using (TimeWarning.New("PoolPhysics.Engine.Tick.ResolveBallCollisions"))
		{
			for (int i = 0; i < balls.Count; i++)
			{
				Data.Ball ball = balls[i];
				if (ball.IsKinematic)
				{
					continue;
				}
				for (int j = 0; j < balls.Count; j++)
				{
					if (i == j)
					{
						continue;
					}
					Data.Ball ball2 = balls[j];
					if (ball2.IsKinematic)
					{
						continue;
					}
					Vector3 val = Vector2.op_Implicit(ball2.Position - ball.Position);
					float magnitude = ((Vector3)(ref val)).magnitude;
					float num = ball.Radius + ball2.Radius;
					if (num > magnitude)
					{
						float num2 = num - magnitude;
						Vector2 val2 = Vector2.op_Implicit(((Vector3)(ref val)).normalized);
						Vector2 val3 = val2 * num2 / 2f;
						ref Vector2 position = ref ball.Position;
						position -= val3;
						ref Vector2 position2 = ref ball2.Position;
						position2 += val3;
						float num3 = Vector2.Dot(ball.Velocity - ball2.Velocity, val2);
						if (num3 > 0f)
						{
							ref Vector2 velocity = ref ball.Velocity;
							velocity -= num3 * val2;
							ref Vector2 velocity2 = ref ball2.Velocity;
							velocity2 += num3 * val2;
							OnBallCollision?.Invoke((ball.Position + ball2.Position) * 0.5f, num3);
						}
						balls[i] = ball;
						balls[j] = ball2;
					}
				}
			}
		}
	}

	private void ResolvePockets()
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("PoolPhysics.Engine.Tick.ResolvePockets"))
		{
			if (pockets.Count == 0)
			{
				return;
			}
			for (int num = balls.Count - 1; num >= 0; num--)
			{
				Data.Ball ball = balls[num];
				if (!ball.IsKinematic)
				{
					for (int i = 0; i < pockets.Count; i++)
					{
						Data.Pocket pocket = pockets[i];
						Vector2 val = ball.Position - pocket.Position;
						if (((Vector2)(ref val)).sqrMagnitude < pocket.Radius * pocket.Radius)
						{
							OnBallPocketed?.Invoke(ball.Id);
						}
					}
				}
			}
		}
	}

	private void RebuildBallIndexLookup()
	{
		ballIndexLookup.Clear();
		for (int i = 0; i < balls.Count; i++)
		{
			ballIndexLookup[balls[i].Id] = i;
		}
	}

	private void Log(string log)
	{
		Debug.Log((object)("[PoolPhysics] " + log));
	}

	private Vector2 ClosestPointOnSegment(Vector2 p, Vector2 a, Vector2 b)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = b - a;
		float num = Vector2.Dot(p - a, val) / ((Vector2)(ref val)).sqrMagnitude;
		num = Mathf.Clamp01(num);
		return a + val * num;
	}

	private bool IntersectRaySegment(Vector2 origin, Vector2 direction, Vector2 a, Vector2 b, out float distance)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		distance = 0f;
		Vector2 b2 = b - a;
		float num = Cross(direction, b2);
		if (Mathf.Abs(num) <= Mathf.Epsilon)
		{
			return false;
		}
		Vector2 a2 = a - origin;
		float num2 = Cross(a2, b2) / num;
		float num3 = Cross(a2, direction) / num;
		if (num2 < 0f)
		{
			return false;
		}
		if (num3 < 0f || num3 > 1f)
		{
			return false;
		}
		distance = num2;
		return true;
	}

	private bool IntersectRayCircle(Vector2 origin, Vector2 direction, Vector2 center, float radius, out float distance)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		distance = 0f;
		Vector2 val = origin - center;
		float num = Vector2.Dot(val, direction);
		float num2 = ((Vector2)(ref val)).sqrMagnitude - radius * radius;
		if (num2 > 0f && num > 0f)
		{
			return false;
		}
		float num3 = num * num - num2;
		if (num3 < 0f)
		{
			return false;
		}
		float num4 = 0f - num - Mathf.Sqrt(num3);
		if (num4 < 0f)
		{
			num4 = 0f;
		}
		distance = num4;
		return true;
	}

	private float Cross(Vector2 a, Vector2 b)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		return a.x * b.y - a.y * b.x;
	}
}

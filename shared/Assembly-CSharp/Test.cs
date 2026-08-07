using System;
using PoolPhysics;
using UnityEngine;

public class Test : MonoBehaviour
{
	[Header("Table")]
	public float TableWidth = 2.54f;

	public float TableHeight = 1.27f;

	[Header("Ball")]
	public float BallRadius = 0.02875f;

	private Engine poolEngine;

	private void Start()
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0292: Unknown result type (might be due to invalid IL or missing references)
		//IL_0301: Unknown result type (might be due to invalid IL or missing references)
		//IL_031c: Unknown result type (might be due to invalid IL or missing references)
		poolEngine = new Engine();
		float num = TableWidth * 0.5f;
		float num2 = TableHeight * 0.5f;
		float num3 = BallRadius * 2f;
		poolEngine.AddWall(MakeWall(new Vector2(0f - num + num3, 0f - num2), new Vector2(0f - num3, 0f - num2)));
		poolEngine.AddWall(MakeWall(new Vector2(num3, 0f - num2), new Vector2(num - num3, 0f - num2)));
		poolEngine.AddWall(MakeWall(new Vector2(0f - num + num3, num2), new Vector2(0f - num3, num2)));
		poolEngine.AddWall(MakeWall(new Vector2(num3, num2), new Vector2(num - num3, num2)));
		poolEngine.AddWall(MakeWall(new Vector2(0f - num, 0f - num2 + num3), new Vector2(0f - num, num2 - num3)));
		poolEngine.AddWall(MakeWall(new Vector2(num, 0f - num2 + num3), new Vector2(num, num2 - num3)));
		Data.Ball ball = new Data.Ball
		{
			Id = 0,
			Position = new Vector2(-1f, 0f),
			Velocity = new Vector2(1f, 0f),
			Radius = BallRadius,
			IsKinematic = false
		};
		Data.Ball ball2 = new Data.Ball
		{
			Id = 1,
			Position = new Vector2(-1f, 0f),
			Velocity = new Vector2(0f, 0f),
			Radius = BallRadius,
			IsKinematic = false
		};
		Data.Ball ball3 = new Data.Ball
		{
			Id = 2,
			Position = new Vector2(0.5f, 0f),
			Velocity = new Vector2(0f, 0f),
			Radius = BallRadius,
			IsKinematic = false
		};
		Data.Ball ball4 = new Data.Ball
		{
			Id = 3,
			Position = new Vector2(-0.5f, 0f),
			Velocity = new Vector2(0f, 0f),
			Radius = BallRadius,
			IsKinematic = false
		};
		Data.Ball ball5 = new Data.Ball
		{
			Id = 4,
			Position = new Vector2(0f, 0.5f),
			Velocity = new Vector2(0f, 0f),
			Radius = BallRadius,
			IsKinematic = false
		};
		poolEngine.AddBall(ball);
		poolEngine.AddBall(ball2);
		poolEngine.AddBall(ball3);
		poolEngine.AddBall(ball4);
		poolEngine.AddBall(ball5);
		poolEngine.ApplyForce(0, new Vector2(5f, 0f));
		poolEngine.ApplyForce(0, new Vector2(-5f, -2f));
	}

	private void Update()
	{
		if (poolEngine.IsReady)
		{
			poolEngine.Tick(Time.deltaTime);
		}
	}

	private void OnDrawGizmos()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		float num = TableWidth * 0.5f;
		float num2 = TableHeight * 0.5f;
		Gizmos.color = Color.green;
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(0f - num, 0f, 0f - num2);
		Vector3 val2 = default(Vector3);
		((Vector3)(ref val2))._002Ector(num, 0f, 0f - num2);
		Vector3 val3 = default(Vector3);
		((Vector3)(ref val3))._002Ector(0f - num, 0f, num2);
		Vector3 val4 = default(Vector3);
		((Vector3)(ref val4))._002Ector(num, 0f, num2);
		Gizmos.DrawLine(val, val2);
		Gizmos.DrawLine(val2, val4);
		Gizmos.DrawLine(val4, val3);
		Gizmos.DrawLine(val3, val);
		if (poolEngine == null)
		{
			return;
		}
		Enumerator<Data.Ball> enumerator = poolEngine.Balls.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				Data.Ball current = enumerator.Current;
				Gizmos.color = ((current.Id == 0) ? Color.white : Color.yellow);
				Gizmos.DrawWireSphere(new Vector3(current.Position.x, 0f, current.Position.y), current.Radius);
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		Gizmos.color = Color.red;
		Enumerator<Data.Wall> enumerator2 = poolEngine.Walls.GetEnumerator();
		try
		{
			Vector3 val6 = default(Vector3);
			while (enumerator2.MoveNext())
			{
				Data.Wall current2 = enumerator2.Current;
				Vector3 val5 = new Vector3(current2.A.x, 0f, current2.A.y);
				((Vector3)(ref val6))._002Ector(current2.B.x, 0f, current2.B.y);
				Gizmos.DrawLine(val5, val6);
			}
		}
		finally
		{
			((IDisposable)enumerator2/*cast due to constrained. prefix*/).Dispose();
		}
	}

	private Data.Wall MakeWall(Vector2 a, Vector2 b)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = b - a;
		Vector2 val2 = new Vector2(0f - val.y, val.x);
		Vector2 normalized = ((Vector2)(ref val2)).normalized;
		return new Data.Wall
		{
			A = a,
			B = b,
			Normal = normalized
		};
	}
}

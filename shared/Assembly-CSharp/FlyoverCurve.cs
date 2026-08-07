using UnityEngine;

public class FlyoverCurve
{
	private const int Samples = 64;

	public Vector3 P0;

	public Vector3 P1;

	public Vector3 P2;

	public float TotalLength;

	public float Duration;

	public float Elapsed;

	public bool Active;

	private float[] arcLengths;

	public void Build(Vector3 p0, Vector3 p1, Vector3 p2, float duration)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		P0 = p0;
		P1 = p1;
		P2 = p2;
		Duration = Mathf.Max(1f, duration);
		Elapsed = 0f;
		Active = true;
		arcLengths = new float[65];
		Vector3 val = p0;
		for (int i = 1; i <= 64; i++)
		{
			Vector3 val2 = Eval(p0, p1, p2, (float)i / 64f);
			arcLengths[i] = arcLengths[i - 1] + Vector3.Distance(val, val2);
			val = val2;
		}
		TotalLength = arcLengths[64];
	}

	public static Vector3 Eval(Vector3 p0, Vector3 p1, Vector3 p2, float t)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		float num = 1f - t;
		return num * num * p0 + 2f * num * t * p1 + t * t * p2;
	}

	public float ElapsedArcDistance()
	{
		return TotalLength * Mathf.Clamp01(Elapsed / Duration);
	}

	public Vector3 EvalAtDistance(float s)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		return Eval(P0, P1, P2, TAtDistance(s));
	}

	public float TAtDistance(float s)
	{
		if (s <= 0f)
		{
			return 0f;
		}
		if (s >= TotalLength)
		{
			return 1f;
		}
		for (int i = 1; i <= 64; i++)
		{
			if (!(s > arcLengths[i]))
			{
				float num = arcLengths[i] - arcLengths[i - 1];
				float num2 = ((num > 0.0001f) ? ((s - arcLengths[i - 1]) / num) : 0f);
				return ((float)(i - 1) + num2) / 64f;
			}
		}
		return 1f;
	}

	public static float ApproximateLength(Vector3 p0, Vector3 p1, Vector3 p2)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		Vector3 val = p0;
		for (int i = 1; i <= 64; i++)
		{
			Vector3 val2 = Eval(p0, p1, p2, (float)i / 64f);
			num += Vector3.Distance(val, val2);
			val = val2;
		}
		return num;
	}
}

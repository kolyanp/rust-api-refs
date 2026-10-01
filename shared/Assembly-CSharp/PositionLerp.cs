using System;
using Development.Attributes;
using Facepunch;
using Rust.Interpolation;
using UnityEngine;

[ResetStaticFields]
public class PositionLerp : IPooled
{
	private static ListHashSet<PositionLerp> InstanceList = new ListHashSet<PositionLerp>();

	public static bool DebugLog = false;

	public static bool DebugDraw = false;

	public static int TimeOffsetInterval = 16;

	public static float TimeOffset = 0f;

	public const int TimeOffsetIntervalMin = 4;

	public const int TimeOffsetIntervalMax = 64;

	private bool enabled = true;

	private Action idleDisable;

	private readonly Interpolator<TransformSnapshot> interpolator = new Interpolator<TransformSnapshot>(32);

	private IPosLerpTarget target;

	private static TransformSnapshot snapshotPrototype = default;

	private float timeOffset0 = float.MaxValue;

	private float timeOffset1 = float.MaxValue;

	private float timeOffset2 = float.MaxValue;

	private float timeOffset3 = float.MaxValue;

	private int timeOffsetCount;

	private float lastClientTime;

	private float lastServerTime;

	private float extrapolatedTime;

	private float enabledTime;

	private Vector3 estimatedVelocityCached;

	private int estimatedVelocityFrame = -1;

	public bool Enabled
	{
		get
		{
			return enabled;
		}
		set
		{
			enabled = value;
			if (enabled)
			{
				OnEnable();
			}
			else
			{
				OnDisable();
			}
		}
	}

	public static float LerpTime => Time.time;

	private void OnEnable()
	{
		InstanceList.Add(this);
		enabledTime = LerpTime;
	}

	private void OnDisable()
	{
		InstanceList.Remove(this);
	}

	public void Initialize(IPosLerpTarget target)
	{
		this.target = target;
		Enabled = true;
	}

	public void Snapshot(Vector3 position, Quaternion rotation, float serverTime)
	{
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		float interpolationDelay = target.GetInterpolationDelay(ILerpInfo.LerpType.Position);
		float interpolationSmoothing = target.GetInterpolationSmoothing();
		float num = interpolationDelay + interpolationSmoothing + 1f;
		float lerpTime = LerpTime;
		timeOffset0 = Mathf.Min(timeOffset0, lerpTime - serverTime);
		timeOffsetCount++;
		if (timeOffsetCount >= TimeOffsetInterval / 4)
		{
			timeOffset3 = timeOffset2;
			timeOffset2 = timeOffset1;
			timeOffset1 = timeOffset0;
			timeOffset0 = float.MaxValue;
			timeOffsetCount = 0;
		}
		TimeOffset = Mathx.Min(timeOffset0, timeOffset1, timeOffset2, timeOffset3);
		lerpTime = serverTime + TimeOffset;
		if (DebugLog && interpolator.list.Count > 0 && serverTime < lastServerTime)
		{
			Debug.LogWarning((object)(target.ToString() + " adding tick from the past: server time " + serverTime + " < " + lastServerTime));
		}
		else if (DebugLog && interpolator.list.Count > 0 && lerpTime < lastClientTime)
		{
			Debug.LogWarning((object)(target.ToString() + " adding tick from the past: client time " + lerpTime + " < " + lastClientTime));
		}
		else
		{
			lastClientTime = lerpTime;
			lastServerTime = serverTime;
			interpolator.Add(new TransformSnapshot(lerpTime, position, rotation));
		}
		interpolator.Cull(lerpTime - num);
	}

	public void Snapshot(Vector3 position, Quaternion rotation)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		Snapshot(position, rotation, LerpTime - TimeOffset);
	}

	public void SnapTo(Vector3 position, Quaternion rotation, float serverTime)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		interpolator.Clear();
		Snapshot(position, rotation, serverTime);
		target.SetNetworkPosition(position);
		target.SetNetworkRotation(rotation);
	}

	public void SnapTo(Vector3 position, Quaternion rotation)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		interpolator.last = new TransformSnapshot(LerpTime, position, rotation);
		Wipe();
	}

	public void SnapToEnd()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		float interpolationDelay = target.GetInterpolationDelay(ILerpInfo.LerpType.Position);
		Interpolator<TransformSnapshot>.Segment segment = interpolator.Query(LerpTime, interpolationDelay, 0f, 0f, ref snapshotPrototype);
		target.SetNetworkPosition(segment.tick.pos);
		target.SetNetworkRotation(segment.tick.rot);
		Wipe();
	}

	public void Wipe()
	{
		interpolator.Clear();
		timeOffsetCount = 0;
		timeOffset0 = float.MaxValue;
		timeOffset1 = float.MaxValue;
		timeOffset2 = float.MaxValue;
		timeOffset3 = float.MaxValue;
	}

	public static void WipeAll()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<PositionLerp> enumerator = InstanceList.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				enumerator.Current.Wipe();
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	protected void DoCycle()
	{
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		if (target == null)
		{
			return;
		}
		float interpolationInertia = target.GetInterpolationInertia();
		float num = ((interpolationInertia > 0f) ? Mathf.InverseLerp(0f, interpolationInertia, LerpTime - enabledTime) : 1f);
		float extrapolationTime = target.GetExtrapolationTime();
		float interpolation = target.GetInterpolationDelay(ILerpInfo.LerpType.Position) * num;
		float num2 = target.GetInterpolationSmoothing() * num;
		Interpolator<TransformSnapshot>.Segment query = interpolator.Query(LerpTime, interpolation, extrapolationTime, num2, ref snapshotPrototype);
		CacheEstimatedVelocity(in query);
		if (query.next.Time >= interpolator.last.Time)
		{
			extrapolatedTime = Mathf.Min(extrapolatedTime + Time.deltaTime, extrapolationTime);
		}
		else
		{
			extrapolatedTime = Mathf.Max(extrapolatedTime - Time.deltaTime, 0f);
		}
		if (extrapolatedTime > 0f && extrapolationTime > 0f && num2 > 0f)
		{
			float num3 = Time.deltaTime / (extrapolatedTime / extrapolationTime * num2);
			query.tick.pos = Vector3.Lerp(target.GetNetworkPosition(), query.tick.pos, num3);
			query.tick.rot = Quaternion.Slerp(target.GetNetworkRotation(), query.tick.rot, num3);
		}
		target.SetNetworkPosition(query.tick.pos);
		target.SetNetworkRotation(query.tick.rot);
		if (DebugDraw)
		{
			target.DrawInterpolationState(query, interpolator.list);
		}
		if (LerpTime - lastClientTime > 10f)
		{
			if (idleDisable == null)
			{
				idleDisable = target.LerpIdleDisable;
			}
			IPosLerpTarget posLerpTarget = target;
			InvokeHandler.Invoke((Behaviour)((posLerpTarget is Behaviour) ? posLerpTarget : null), idleDisable, 0f);
		}
	}

	public void TransformEntries(Matrix4x4 matrix)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		Quaternion rotation = matrix.rotation;
		for (int i = 0; i < interpolator.list.Count; i++)
		{
			TransformSnapshot value = interpolator.list[i];
			value.pos = matrix.MultiplyPoint3x4(value.pos);
			value.rot = rotation * value.rot;
			interpolator.list[i] = value;
		}
		interpolator.last.pos = matrix.MultiplyPoint3x4(interpolator.last.pos);
		interpolator.last.rot = rotation * interpolator.last.rot;
	}

	public Quaternion GetEstimatedAngularVelocity()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		if (target == null)
		{
			return Quaternion.identity;
		}
		float extrapolationTime = target.GetExtrapolationTime();
		float interpolationDelay = target.GetInterpolationDelay(ILerpInfo.LerpType.Position);
		float interpolationSmoothing = target.GetInterpolationSmoothing();
		Interpolator<TransformSnapshot>.Segment segment = interpolator.Query(LerpTime, interpolationDelay, extrapolationTime, interpolationSmoothing, ref snapshotPrototype);
		TransformSnapshot next = segment.next;
		TransformSnapshot prev = segment.prev;
		if (next.Time == prev.Time)
		{
			return Quaternion.identity;
		}
		return Quaternion.Euler((prev.rot.eulerAngles - next.rot.eulerAngles) / (prev.Time - next.Time));
	}

	public Vector3 GetEstimatedVelocity()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (target == null)
		{
			return Vector3.zero;
		}
		if (estimatedVelocityFrame == Time.frameCount)
		{
			return estimatedVelocityCached;
		}
		float extrapolationTime = target.GetExtrapolationTime();
		float interpolationDelay = target.GetInterpolationDelay(ILerpInfo.LerpType.Position);
		float interpolationSmoothing = target.GetInterpolationSmoothing();
		CacheEstimatedVelocity(interpolator.Query(LerpTime, interpolationDelay, extrapolationTime, interpolationSmoothing, ref snapshotPrototype));
		return estimatedVelocityCached;
	}

	private void CacheEstimatedVelocity(in Interpolator<TransformSnapshot>.Segment query)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		TransformSnapshot next = query.next;
		TransformSnapshot prev = query.prev;
		estimatedVelocityCached = ((next.Time == prev.Time) ? Vector3.zero : ((prev.pos - next.pos) / (prev.Time - next.Time)));
		estimatedVelocityFrame = Time.frameCount;
	}

	void IPooled.EnterPool()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		target = null;
		idleDisable = null;
		estimatedVelocityCached = Vector3.zero;
		estimatedVelocityFrame = -1;
		interpolator.Clear();
		timeOffset0 = float.MaxValue;
		timeOffset1 = float.MaxValue;
		timeOffset2 = float.MaxValue;
		timeOffset3 = float.MaxValue;
		lastClientTime = 0f;
		lastServerTime = 0f;
		extrapolatedTime = 0f;
		timeOffsetCount = 0;
		Enabled = false;
	}

	void IPooled.LeavePool()
	{
	}

	public static void Clear()
	{
		InstanceList.Clear();
	}

	public static void Cycle()
	{
		PositionLerp[] buffer = InstanceList.Values.Buffer;
		int count = InstanceList.Count;
		for (int i = 0; i < count; i++)
		{
			buffer[i].DoCycle();
		}
	}
}

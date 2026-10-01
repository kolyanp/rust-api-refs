using UnityEngine;

namespace Rust.Ai.Gen2;

internal static class NPCMountMontage
{
	private const float MinimumMotion = 0.01f;

	private const float MinimumRotation = 1f;

	public static RootMotionPlayer.Warp BuildWarp(RootMotionData data, MountMontageWindow window, Quaternion initialRotation, Vector3 from, Vector3 to, Quaternion toRotation, float playbackSpeed = 1f)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		GetTravelWindow(data, window, playbackSpeed, out var start, out var end, out var lastSampled);
		Vector3 val = new Vector3(Travel(data.xMotionCurve, start, end), Travel(data.yMotionCurve, start, end), Travel(data.zMotionCurve, start, end));
		Vector3 val2 = new Vector3(Unwarped(data.xMotionCurve, start, end, lastSampled), Unwarped(data.yMotionCurve, start, end, lastSampled), Unwarped(data.zMotionCurve, start, end, lastSampled));
		Vector3 val3 = Quaternion.Inverse(initialRotation) * (to - from) - val2;
		Vector3 translationScale = new Vector3(Scale(val3.x, val.x), Scale(val3.y, val.y), Scale(val3.z, val.z));
		float num = Mathf.DeltaAngle(initialRotation.eulerAngles.y, toRotation.eulerAngles.y) - Unwarped(data.yRotationCurve, start, end, lastSampled);
		float num2 = Travel(data.yRotationCurve, start, end);
		float rotationScale = ((Mathf.Abs(num2) < 1f) ? 1f : (num / num2));
		return new RootMotionPlayer.Warp(start, end, translationScale, rotationScale);
	}

	private static void GetTravelWindow(RootMotionData data, MountMontageWindow window, float playbackSpeed, out float start, out float end, out float lastSampled)
	{
		if (!window.IsAuthored)
		{
			window = MountMontageWindow.Whole;
		}
		float length = data.inPlaceAnimation.length;
		lastSampled = Mathf.Max(0f, length - 0.25f * playbackSpeed);
		end = Mathf.Min(window.end * length, lastSampled);
		start = Mathf.Clamp(window.start * length, 0f, end);
	}

	private static float Travel(AnimationCurve curve, float start, float end)
	{
		if (curve == null || curve.length == 0)
		{
			return 0f;
		}
		return curve.Evaluate(end) - curve.Evaluate(start);
	}

	private static float Unwarped(AnimationCurve curve, float start, float end, float lastSampled)
	{
		return Travel(curve, 0f, start) + Travel(curve, end, lastSampled);
	}

	private static float Scale(float wanted, float total)
	{
		if (!(Mathf.Abs(total) < 0.01f))
		{
			return wanted / total;
		}
		return 1f;
	}
}

using System;
using UnityEngine;

namespace Rust.Ai.Gen2;

public static class LivestockSunPath
{
	public const float RayLift = 0.5f;

	public const float RayLength = 200f;

	public const int RayMask = 144769025;

	private const float DawnMargin = 0.5f;

	private const float FallbackSunrise = 6f;

	private const float FallbackSunset = 18f;

	private const int DaylightShareSamples = 96;

	public static bool SeesTheSunNow(Vector3 position)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		return !Physics.Raycast(position + Vector3.up * 0.5f, TOD_Sky.Instance.SunDirection, 200f, 144769025, (QueryTriggerInteraction)1);
	}

	public static Vector3 DirectionAt(float hour)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		TOD_Sky instance = TOD_Sky.Instance;
		OrbitAt(instance, hour, out var zenith, out var azimuth);
		Vector3 val = instance.OrbitalToLocal(zenith, azimuth);
		return instance.Components.DomeTransform.TransformDirection(val);
	}

	private static bool IsDayAt(TOD_Sky sky, float hour)
	{
		OrbitAt(sky, hour, out var zenith, out var _);
		return Mathf.InverseLerp(105f, 90f, 57.29578f * zenith) > 0.1f;
	}

	private static void OrbitAt(TOD_Sky sky, float hour, out float zenith, out float azimuth)
	{
		float num = MathF.PI / 180f * sky.World.Latitude;
		float num2 = Mathf.Sin(num);
		float num3 = Mathf.Cos(num);
		float longitude = sky.World.Longitude;
		int year = sky.Cycle.Year;
		int month = sky.Cycle.Month;
		int day = sky.Cycle.Day;
		float num4 = hour - sky.World.UTC;
		float num5 = (float)(367 * year - 7 * (year + (month + 9) / 12) / 4 + 275 * month / 9 + day - 730530) + num4 / 24f;
		float num6 = MathF.PI / 180f * (23.4393f - 3.563E-07f * num5);
		float num7 = 282.9404f + 4.70935E-05f * num5;
		float num8 = 0.016709f - 1.151E-09f * num5;
		float num9 = MathF.PI / 180f * (356.047f + 0.98560023f * num5);
		float num10 = num9 + num8 * Mathf.Sin(num9) * (1f + num8 * Mathf.Cos(num9));
		float num11 = Mathf.Cos(num10) - num8;
		float num12 = Mathf.Sqrt(1f - num8 * num8) * Mathf.Sin(num10);
		float num13 = 57.29578f * Mathf.Atan2(num12, num11);
		float num14 = Mathf.Sqrt(num11 * num11 + num12 * num12);
		float num15 = MathF.PI / 180f * (num13 + num7);
		float num16 = num14 * Mathf.Cos(num15);
		float num17 = num14 * Mathf.Sin(num15);
		float num18 = num16;
		float num19 = num17 * Mathf.Cos(num6);
		float num20 = num17 * Mathf.Sin(num6);
		float num21 = Mathf.Atan2(num19, num18);
		float num22 = Mathf.Atan2(num20, Mathf.Sqrt(num18 * num18 + num19 * num19));
		float num23 = Mathf.Sin(num22);
		float num24 = Mathf.Cos(num22);
		float num25 = num13 + num7 + 180f + 15f * num4;
		float num26 = MathF.PI / 180f * (num25 + longitude) - num21;
		float num27 = Mathf.Cos(num26) * num24;
		float num28 = Mathf.Sin(num26) * num24;
		float num29 = num23;
		float num30 = num27 * num2 - num29 * num3;
		float num31 = num28;
		float num32 = num27 * num3 + num29 * num2;
		azimuth = Mathf.Atan2(num31, num30) + MathF.PI;
		float num33 = Mathf.Atan2(num32, Mathf.Sqrt(num30 * num30 + num31 * num31));
		zenith = MathF.PI / 2f - num33;
	}

	public static void DaylightHours(int count, float[] hours)
	{
		SunriseAndSunset(TOD_Sky.Instance, 1f, out var sunrise, out var sunset);
		float num = sunrise + 0.5f;
		float num2 = sunset - 0.5f - num;
		for (int i = 0; i < count; i++)
		{
			hours[i] = num + num2 * ((float)i + 0.5f) / (float)count;
		}
	}

	public static float RealDaylightShare()
	{
		TOD_Sky instance = TOD_Sky.Instance;
		if ((Object)(object)instance == (Object)null)
		{
			return 0.5f;
		}
		TOD_Time time = instance.Components.Time;
		bool flag = (Object)(object)time != (Object)null && time.UseTimeCurve;
		int num = 0;
		for (int i = 0; i < 96; i++)
		{
			float num2 = 24f * ((float)i + 0.5f) / 96f;
			if (IsDayAt(instance, flag ? time.EvaulateTimeCurve(num2) : num2))
			{
				num++;
			}
		}
		return (float)num / 96f;
	}

	private static void SunriseAndSunset(TOD_Sky sky, float shortestDay, out float sunrise, out float sunset)
	{
		sunrise = (((Object)(object)sky != (Object)null) ? sky.SunriseTime : float.NaN);
		sunset = (((Object)(object)sky != (Object)null) ? sky.SunsetTime : float.NaN);
		if (float.IsNaN(sunrise) || float.IsNaN(sunset) || sunset - sunrise <= shortestDay)
		{
			sunrise = 6f;
			sunset = 18f;
		}
	}
}

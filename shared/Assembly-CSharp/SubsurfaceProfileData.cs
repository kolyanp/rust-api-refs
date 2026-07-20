using System;
using UnityEngine;

[Serializable]
public struct SubsurfaceProfileData
{
	[Range(0.1f, 100f)]
	public float ScatterRadius;

	[ColorUsage(false, false)]
	public Color SubsurfaceColor;

	[ColorUsage(false, false)]
	public Color FalloffColor;

	[ColorUsage(false, true)]
	public Color TransmissionTint;

	public static SubsurfaceProfileData Default => new SubsurfaceProfileData
	{
		ScatterRadius = 1.2f,
		SubsurfaceColor = new Color(0.48f, 0.41f, 0.28f),
		FalloffColor = new Color(1f, 0.37f, 0.3f),
		TransmissionTint = new Color(0.48f, 0.41f, 0.28f)
	};

	public static SubsurfaceProfileData Invalid => new SubsurfaceProfileData
	{
		ScatterRadius = 0f,
		SubsurfaceColor = Color.clear,
		FalloffColor = Color.clear,
		TransmissionTint = Color.clear
	};
}

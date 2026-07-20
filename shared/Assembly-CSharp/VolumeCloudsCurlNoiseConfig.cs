using System;
using UnityEngine;

[Serializable]
public class VolumeCloudsCurlNoiseConfig
{
	public Vector2 Frequency = Vector2.one;

	public float Strength;

	public int Octaves = 1;

	public void CopyFrom(VolumeCloudsCurlNoiseConfig copy)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		Frequency = copy.Frequency;
		Strength = copy.Strength;
		Octaves = copy.Octaves;
	}
}

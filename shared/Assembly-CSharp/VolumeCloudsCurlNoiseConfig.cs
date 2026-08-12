using System;
using UnityEngine;

[Serializable]
public class VolumeCloudsCurlNoiseConfig
{
	public Vector2 Frequency;

	public float Strength;

	public int Octaves;

	public void CopyFrom(VolumeCloudsCurlNoiseConfig copy)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		Frequency = copy.Frequency;
		Strength = copy.Strength;
		Octaves = copy.Octaves;
	}

	public VolumeCloudsCurlNoiseConfig()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		Frequency = Vector2.one;
		Octaves = 1;
		base._002Ector();
	}
}

using System;

[Serializable]
public struct NoiseParameters(int octaves, float frequency, float amplitude, float offset)
{
	public int Octaves = octaves;

	public float Frequency = frequency;

	public float Amplitude = amplitude;

	public float Offset = offset;
}

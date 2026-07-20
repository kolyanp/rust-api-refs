using System;

namespace FIMSpace.Generating;

[Serializable]
public struct MinMaxF(float min, float max)
{
	public float Min = min;

	public float Max = max;
}

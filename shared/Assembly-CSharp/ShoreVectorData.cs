using UnityEngine;

public class ShoreVectorData : BaseScriptableObject
{
	[ReadOnly]
	public float WorldSize;

	[ReadOnly]
	[Header("Shore Vectors")]
	public float[] Distances;

	[ReadOnly]
	public Vector4[] Vectors;

	[Header("Slope Data")]
	[ReadOnly]
	public Vector2[] SlopeData;

	[Header("WaterHeight")]
	[ReadOnly]
	public float[] WaterHeightData;

	[Header("HeightData")]
	[ReadOnly]
	public short[] HeightData;

	[ReadOnly]
	public Vector2 HeightInfo;

	public int ShoreVectorDimension => (int)Mathf.Sqrt((float)(Distances?.Length ?? 0));

	public int SlopeDataDimension => (int)Mathf.Sqrt((float)(SlopeData?.Length ?? 0));

	public int WaterHeightDimension => (int)Mathf.Sqrt((float)(WaterHeightData?.Length ?? 0));

	public int HeightDimension => (int)Mathf.Sqrt((float)(HeightData?.Length ?? 0));
}

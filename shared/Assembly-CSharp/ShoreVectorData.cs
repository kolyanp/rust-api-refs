using UnityEngine;

public class ShoreVectorData : BaseScriptableObject
{
	[ReadOnly]
	public float WorldSize;

	[Header("Shore Vectors")]
	[ReadOnly]
	public float[] Distances;

	[ReadOnly]
	public Vector4[] Vectors;

	[ReadOnly]
	[Header("Slope Data")]
	public Vector2[] SlopeData;

	[ReadOnly]
	[Header("WaterHeight")]
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

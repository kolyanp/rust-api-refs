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

	[ReadOnly]
	[Header("WaterHeight")]
	public float[] WaterHeightData;

	[Header("HeightData")]
	[ReadOnly]
	public short[] HeightData;

	[ReadOnly]
	public Vector2 HeightInfo;

	public int ShoreVectorDimension
	{
		get
		{
			float[] distances = Distances;
			return (int)Mathf.Sqrt((float)((distances != null) ? distances.Length : 0));
		}
	}

	public int SlopeDataDimension
	{
		get
		{
			Vector2[] slopeData = SlopeData;
			return (int)Mathf.Sqrt((float)((slopeData != null) ? slopeData.Length : 0));
		}
	}

	public int WaterHeightDimension
	{
		get
		{
			float[] waterHeightData = WaterHeightData;
			return (int)Mathf.Sqrt((float)((waterHeightData != null) ? waterHeightData.Length : 0));
		}
	}

	public int HeightDimension
	{
		get
		{
			short[] heightData = HeightData;
			return (int)Mathf.Sqrt((float)((heightData != null) ? heightData.Length : 0));
		}
	}
}

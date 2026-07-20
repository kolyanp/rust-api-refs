using System;
using UnityEngine;

[Serializable]
public struct LayerSelect(int layer)
{
	[SerializeField]
	private int layer = layer;

	public int Mask => 1 << layer;

	public string Name => LayerMask.LayerToName(layer);

	public static implicit operator int(LayerSelect layer)
	{
		return layer.layer;
	}

	public static implicit operator LayerSelect(int layer)
	{
		return new LayerSelect(layer);
	}
}

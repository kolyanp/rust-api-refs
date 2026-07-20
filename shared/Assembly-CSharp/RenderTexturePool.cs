using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public class RenderTexturePool
{
	private readonly Queue<RenderTexture> inactive = new Queue<RenderTexture>();

	private readonly HashSet<RenderTexture> active = new HashSet<RenderTexture>();

	public RenderTexturePool(int width, int height, GraphicsFormat graphicsFormat, TextureDimension textureDimension, FilterMode filterMode, int capacity)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		for (int i = 0; i < capacity; i++)
		{
			RenderTexture val = new RenderTexture(width, height, 0, graphicsFormat, 0)
			{
				dimension = textureDimension,
				filterMode = filterMode,
				anisoLevel = 0
			};
			val.Create();
			inactive.Enqueue(val);
		}
	}

	public RenderTexture GetInstance()
	{
		if (inactive.Count <= 0)
		{
			return null;
		}
		RenderTexture val = inactive.Dequeue();
		active.Add(val);
		return val;
	}

	public void ReleaseInstance(RenderTexture renderTexture)
	{
		if (!((Object)(object)renderTexture == (Object)null))
		{
			active.Remove(renderTexture);
			inactive.Enqueue(renderTexture);
		}
	}
}

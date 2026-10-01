using System;
using UnityEngine;

[Serializable]
public class LODGroupRenderers
{
	public LODGroup lodGroup;

	public Renderer[] renderers;

	public void SetRenderersEnabled(bool enabled)
	{
		Renderer[] array = renderers;
		foreach (Renderer val in array)
		{
			if ((Object)(object)val != (Object)null)
			{
				val.enabled = enabled;
			}
		}
	}
}

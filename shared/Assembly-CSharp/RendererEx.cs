using System;
using System.Collections.Generic;
using UnityEngine;

public static class RendererEx
{
	private static readonly Memoized<Material[], int> ArrayCache = new Memoized<Material[], int>((Func<int, Material[]>)((int n) => (Material[])(object)new Material[n]));

	public static void SetSharedMaterials(this Renderer renderer, List<Material> materials)
	{
		if (materials.Count != 0)
		{
			if (materials.Count > 10)
			{
				throw new ArgumentOutOfRangeException("materials");
			}
			Material[] array = ArrayCache.Get(materials.Count);
			for (int i = 0; i < materials.Count; i++)
			{
				array[i] = materials[i];
			}
			renderer.sharedMaterials = array;
		}
	}

	public static MaterialPropertyBlock[] GetMaterialPropertyBlocksUnsafe(this Renderer renderer, MaterialPropertyBlock[] per_material_blocks)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		if (!renderer.HasPropertyBlock())
		{
			return null;
		}
		if (per_material_blocks == null || renderer.sharedMaterials.Length > per_material_blocks.Length)
		{
			Array.Resize(ref per_material_blocks, Mathf.NextPowerOfTwo(renderer.sharedMaterials.Length));
			for (int i = 0; i < per_material_blocks.Length; i++)
			{
				ref MaterialPropertyBlock reference = ref per_material_blocks[i];
				if (reference == null)
				{
					reference = new MaterialPropertyBlock();
				}
			}
		}
		for (int j = 0; j < renderer.sharedMaterials.Length; j++)
		{
			renderer.GetPropertyBlock(per_material_blocks[j], j);
			if (per_material_blocks[j].isEmpty)
			{
				renderer.GetPropertyBlock(per_material_blocks[j]);
			}
		}
		return per_material_blocks;
	}

	public static MaterialPropertyBlock GetRendererPropertyBlock(this Renderer renderer)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Expected O, but got Unknown
		if (renderer.HasPropertyBlock())
		{
			MaterialPropertyBlock val = new MaterialPropertyBlock();
			renderer.GetPropertyBlock(val);
			return val;
		}
		return null;
	}

	public static MaterialPropertyBlock[] GetMaterialPropertyBlocks(this Renderer renderer)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		if (renderer.HasPropertyBlock() && renderer.sharedMaterials != null && renderer.sharedMaterials.Length != 0)
		{
			MaterialPropertyBlock[] array = (MaterialPropertyBlock[])(object)new MaterialPropertyBlock[renderer.sharedMaterials.Length];
			for (int i = 0; i < renderer.sharedMaterials.Length; i++)
			{
				array[i] = new MaterialPropertyBlock();
				renderer.GetPropertyBlock(array[i], i);
				if (array[i].isEmpty)
				{
					renderer.GetPropertyBlock(array[i]);
				}
			}
			return array;
		}
		return null;
	}
}

using System;
using System.Collections.Generic;
using Facepunch;
using UnityEngine;

[RequireComponent(typeof(ViewmodelClothing))]
public class ViewmodelClothingRandomiser : MonoBehaviour
{
	[Serializable]
	public struct SkinCollection
	{
		public SkeletonSkin[] skins;
	}

	[Serializable]
	public struct SkinRandomizer
	{
		[Tooltip("One entry per group in the matching wearable randomizer, in the same order. If the lengths don't match, first person won't match third person.")]
		public SkinCollection[] groups;
	}

	[Tooltip("One entry per set in the wearable prefab's randomParts, in the same order. Leave a set's groups empty if it doesn't affect the arms.")]
	public SkinRandomizer[] randomSkins;

	public IEnumerable<SkeletonSkin> AllSkins()
	{
		if (randomSkins == null)
		{
			yield break;
		}
		SkinRandomizer[] array = randomSkins;
		for (int i = 0; i < array.Length; i++)
		{
			SkinRandomizer skinRandomizer = array[i];
			if (skinRandomizer.groups == null)
			{
				continue;
			}
			SkinCollection[] groups = skinRandomizer.groups;
			for (int j = 0; j < groups.Length; j++)
			{
				SkinCollection skinCollection = groups[j];
				if (skinCollection.skins == null)
				{
					continue;
				}
				SkeletonSkin[] skins = skinCollection.skins;
				foreach (SkeletonSkin val in skins)
				{
					if ((Object)(object)val != (Object)null)
					{
						yield return val;
					}
				}
			}
		}
	}
}

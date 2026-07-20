using System;
using UnityEngine;

public class LODStripper
{
	private static void StripLods(GameObject target)
	{
		StripLods(target, new Type[5]
		{
			typeof(Mesh),
			typeof(Collision),
			typeof(Animator),
			typeof(DrawSkeleton),
			typeof(ObjectMotionVectorFix)
		});
	}

	public static void StripLods(GameObject target, Type[] keepComponents)
	{
		if ((Object)(object)target == (Object)null)
		{
			Debug.LogWarning((object)"You have to select something first Paddy...");
			return;
		}
		RendererLOD[] componentsInChildren = target.GetComponentsInChildren<RendererLOD>();
		LODGroup[] componentsInChildren2 = target.GetComponentsInChildren<LODGroup>();
		RendererLOD[] array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			StripLod(array[i]);
		}
		LODGroup[] array2 = componentsInChildren2;
		for (int i = 0; i < array2.Length; i++)
		{
			StripLod(array2[i]);
		}
		StripLod(target.GetComponent<RendererLOD>());
		StripLod(target.GetComponent<LODGroup>());
		StripComponentsNotOfType(target, keepComponents);
	}

	private static void StripLod(RendererLOD lod)
	{
		if ((Object)(object)lod == (Object)null)
		{
			return;
		}
		for (int i = 1; i < lod.States.Length; i++)
		{
			if ((Object)(object)lod.States[i].renderer != (Object)null)
			{
				Destroy((Object)(object)((Component)lod.States[i].renderer).gameObject);
			}
		}
		Destroy((Object)(object)lod);
	}

	private static void StripLod(LODGroup lod)
	{
		if ((Object)(object)lod == (Object)null)
		{
			return;
		}
		LOD[] lODs = lod.GetLODs();
		for (int i = 1; i < lODs.Length; i++)
		{
			Renderer[] renderers = lODs[i].renderers;
			foreach (Renderer val in renderers)
			{
				if ((Object)(object)val != (Object)null)
				{
					Destroy((Object)(object)((Component)val).gameObject);
				}
			}
		}
		Destroy((Object)(object)lod);
	}

	private static void Destroy(Object target)
	{
		if (!Application.isPlaying)
		{
			Object.DestroyImmediate(target);
		}
		else
		{
			Object.Destroy(target);
		}
	}

	private static void StripComponentsOfType(GameObject go, Type component)
	{
		Component component2 = go.GetComponent(component);
		if ((Object)(object)component2 != (Object)null)
		{
			Destroy((Object)(object)component2);
		}
		Component[] componentsInChildren = go.GetComponentsInChildren(component);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Destroy((Object)(object)componentsInChildren[i]);
		}
	}

	private static void StripComponentsNotOfType(GameObject go, Type[] components)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		MonoBehaviour[] components2 = go.GetComponents<MonoBehaviour>();
		foreach (MonoBehaviour target in components2)
		{
			if (IsComponentNotOfType(target, components))
			{
				Destroy((Object)(object)target);
			}
		}
		foreach (Transform item in go.transform)
		{
			StripComponentsNotOfType(((Component)item).gameObject, components);
		}
	}

	private static bool IsComponentNotOfType(MonoBehaviour target, Type[] components)
	{
		foreach (Type type in components)
		{
			if (type == ((object)target).GetType() || ((object)target).GetType().IsSubclassOf(type))
			{
				return false;
			}
		}
		return true;
	}
}

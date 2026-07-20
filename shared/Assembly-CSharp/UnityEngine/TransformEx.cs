using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Development.Attributes;
using Facepunch;

namespace UnityEngine;

public static class TransformEx
{
	public static string GetRecursiveName(this Transform transform, string strEndName = "")
	{
		string text = ((Object)transform).name;
		if (!string.IsNullOrEmpty(strEndName))
		{
			text = text + "/" + strEndName;
		}
		if ((Object)(object)transform.parent != (Object)null)
		{
			text = GetRecursiveName(transform.parent, text);
		}
		return text;
	}

	public static void RemoveComponent<T>(this Transform transform) where T : Component
	{
		T component = ((Component)transform).GetComponent<T>();
		if (!((Object)(object)component == (Object)null))
		{
			GameManager.Destroy((Component)(object)component);
		}
	}

	public static void RetireAllChildren(this Transform transform, GameManager gameManager)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		List<GameObject> list = Pool.Get<List<GameObject>>();
		foreach (Transform item in transform)
		{
			Transform val = item;
			if (!((Component)val).CompareTag("persist"))
			{
				list.Add(((Component)val).gameObject);
			}
		}
		foreach (GameObject item2 in list)
		{
			gameManager.Retire(item2);
		}
		Pool.FreeUnmanaged<GameObject>(ref list);
	}

	public static List<Transform> GetChildren(this Transform transform)
	{
		return ((IEnumerable)transform).Cast<Transform>().ToList();
	}

	public static void OrderChildren(this Transform tx, Func<Transform, object> selector)
	{
		foreach (Transform item in ((IEnumerable)tx).Cast<Transform>().OrderBy(selector))
		{
			item.SetAsLastSibling();
		}
	}

	public static List<Transform> GetAllChildren(this Transform transform)
	{
		List<Transform> list = new List<Transform>();
		if ((Object)(object)transform != (Object)null)
		{
			AddAllChildren(transform, list);
		}
		return list;
	}

	[PoolAnalyzerNonCaching]
	public static void AddAllChildren(this Transform transform, List<Transform> list)
	{
		list.Add(transform);
		for (int i = 0; i < transform.childCount; i++)
		{
			Transform child = transform.GetChild(i);
			if (!((Object)(object)child == (Object)null))
			{
				AddAllChildren(child, list);
			}
		}
	}

	public static Transform[] GetChildrenWithTag(this Transform transform, string strTag)
	{
		return (from x in GetAllChildren(transform)
			where ((Component)x).CompareTag(strTag)
			select x).ToArray();
	}

	public static Matrix4x4 LocalToPrefabRoot(this Transform transform)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		Matrix4x4 val = Matrix4x4.identity;
		while ((Object)(object)transform.parent != (Object)null)
		{
			val *= Matrix4x4.TRS(transform.localPosition, transform.localRotation, transform.localScale);
			transform = transform.parent;
		}
		return val;
	}

	public static void Identity(this GameObject go, bool resetScale = true)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		Transform transform = go.transform;
		transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
		if (resetScale)
		{
			transform.localScale = Vector3.one;
		}
	}

	public static GameObject CreateChild(this GameObject go)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_001e: Expected O, but got Unknown
		GameObject val = new GameObject();
		val.transform.parent = go.transform;
		Identity(val);
		return val;
	}

	public static GameObject InstantiateChild(this GameObject go, GameObject prefab)
	{
		GameObject obj = Instantiate.GameObject(prefab, (Transform)null);
		obj.transform.SetParent(go.transform, false);
		Identity(obj);
		return obj;
	}

	public static void SetLayerRecursive(this GameObject go, int Layer)
	{
		if (go.layer != Layer)
		{
			go.layer = Layer;
		}
		for (int i = 0; i < go.transform.childCount; i++)
		{
			SetLayerRecursive(((Component)go.transform.GetChild(i)).gameObject, Layer);
		}
	}

	public static bool DropToGround(this Transform transform, bool alignToNormal = false, float fRange = 100f)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (GetGroundInfo(transform, out var pos, out var normal, fRange))
		{
			transform.position = pos;
			if (alignToNormal)
			{
				transform.rotation = Quaternion.LookRotation(transform.forward, normal);
			}
			return true;
		}
		return false;
	}

	public static bool GetGroundInfo(this Transform transform, out Vector3 pos, out Vector3 normal, float range = 100f)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return TransformUtil.GetGroundInfo(transform.position, out pos, out normal, range, transform);
	}

	public static bool GetGroundInfoTerrainOnly(this Transform transform, out Vector3 pos, out Vector3 normal, float range = 100f)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return TransformUtil.GetGroundInfoTerrainOnly(transform.position, out pos, out normal, range);
	}

	public static Bounds WorkoutRenderBounds(this Transform tx)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		Bounds bounds = default(Bounds);
		((Bounds)(ref bounds))._002Ector(Vector3.zero, Vector3.zero);
		Renderer[] componentsInChildren = ((Component)tx).GetComponentsInChildren<Renderer>();
		foreach (Renderer val in componentsInChildren)
		{
			if (!(val is ParticleSystemRenderer))
			{
				if (((Bounds)(ref bounds)).center == Vector3.zero)
				{
					bounds = val.bounds;
				}
				else
				{
					((Bounds)(ref bounds)).Encapsulate(val.bounds);
				}
			}
		}
		return bounds;
	}

	public static List<T> GetSiblings<T>(this Transform transform, bool includeSelf = false)
	{
		List<T> list = new List<T>();
		if ((Object)(object)transform.parent == (Object)null)
		{
			return list;
		}
		for (int i = 0; i < transform.parent.childCount; i++)
		{
			Transform child = transform.parent.GetChild(i);
			if (includeSelf || !((Object)(object)child == (Object)(object)transform))
			{
				T component = ((Component)child).GetComponent<T>();
				if (component != null)
				{
					list.Add(component);
				}
			}
		}
		return list;
	}

	public static void DestroyChildren(this Transform transform)
	{
		for (int i = 0; i < transform.childCount; i++)
		{
			GameManager.Destroy(((Component)transform.GetChild(i)).gameObject);
		}
	}

	public static void SetChildrenActive(this Transform transform, bool b)
	{
		for (int i = 0; i < transform.childCount; i++)
		{
			((Component)transform.GetChild(i)).gameObject.SetActive(b);
		}
	}

	public static Transform ActiveChild(this Transform transform, string name, bool bDisableOthers)
	{
		Transform result = null;
		for (int i = 0; i < transform.childCount; i++)
		{
			Transform child = transform.GetChild(i);
			if (((Object)child).name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
			{
				result = child;
				((Component)child).gameObject.SetActive(true);
			}
			else if (bDisableOthers)
			{
				((Component)child).gameObject.SetActive(false);
			}
		}
		return result;
	}

	public static T GetComponentInChildrenIncludeDisabled<T>(this Transform transform) where T : Component
	{
		List<T> list = Pool.Get<List<T>>();
		((Component)transform).GetComponentsInChildren<T>(true, list);
		T result = ((list.Count > 0) ? list[0] : default(T));
		Pool.FreeUnmanaged<T>(ref list);
		return result;
	}

	public static bool HasComponentInChildrenIncludeDisabled<T>(this Transform transform) where T : Component
	{
		List<T> list = Pool.Get<List<T>>();
		((Component)transform).GetComponentsInChildren<T>(true, list);
		bool result = list.Count > 0;
		Pool.FreeUnmanaged<T>(ref list);
		return result;
	}

	public static Bounds GetBounds(this Transform transform, bool includeRenderers = true, bool includeColliders = true, bool includeInactive = true, bool centerAtZero = true)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		Bounds result = default(Bounds);
		((Bounds)(ref result))._002Ector(Vector3.zero, Vector3.zero);
		if (includeRenderers)
		{
			MeshLOD[] componentsInChildren = ((Component)transform).GetComponentsInChildren<MeshLOD>(includeInactive);
			foreach (MeshLOD meshLOD in componentsInChildren)
			{
				Mesh highestDetailMesh = meshLOD.GetHighestDetailMesh();
				if ((Object)(object)highestDetailMesh != (Object)null)
				{
					Matrix4x4 matrix = transform.worldToLocalMatrix * ((Component)meshLOD).transform.localToWorldMatrix;
					Bounds val = BoundsEx.Transform(highestDetailMesh.bounds, matrix);
					if (!flag && !centerAtZero)
					{
						result = val;
						flag = true;
					}
					((Bounds)(ref result)).Encapsulate(val);
				}
			}
			MeshFilter[] componentsInChildren2 = ((Component)transform).GetComponentsInChildren<MeshFilter>(includeInactive);
			foreach (MeshFilter val2 in componentsInChildren2)
			{
				if (Object.op_Implicit((Object)(object)val2.sharedMesh))
				{
					Matrix4x4 matrix2 = transform.worldToLocalMatrix * ((Component)val2).transform.localToWorldMatrix;
					Bounds bounds = val2.sharedMesh.bounds;
					Bounds val3 = BoundsEx.Transform(bounds, matrix2);
					if (!flag && !centerAtZero)
					{
						result = val3;
						flag = true;
					}
					((Bounds)(ref result)).Encapsulate(BoundsEx.Transform(bounds, matrix2));
				}
			}
			SkinnedMeshRenderer[] componentsInChildren3 = ((Component)transform).GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive);
			foreach (SkinnedMeshRenderer val4 in componentsInChildren3)
			{
				if (Object.op_Implicit((Object)(object)val4.sharedMesh))
				{
					Matrix4x4 matrix3 = transform.worldToLocalMatrix * ((Component)val4).transform.localToWorldMatrix;
					Bounds val5 = BoundsEx.Transform(val4.sharedMesh.bounds, matrix3);
					if (!flag && !centerAtZero)
					{
						result = val5;
						flag = true;
					}
					((Bounds)(ref result)).Encapsulate(val5);
				}
			}
		}
		if (includeColliders)
		{
			MeshCollider[] componentsInChildren4 = ((Component)transform).GetComponentsInChildren<MeshCollider>(includeInactive);
			foreach (MeshCollider val6 in componentsInChildren4)
			{
				if (Object.op_Implicit((Object)(object)val6.sharedMesh) && !((Collider)val6).isTrigger)
				{
					Matrix4x4 matrix4 = transform.worldToLocalMatrix * ((Component)val6).transform.localToWorldMatrix;
					Bounds val7 = BoundsEx.Transform(val6.sharedMesh.bounds, matrix4);
					if (!flag && !centerAtZero)
					{
						result = val7;
						flag = true;
					}
					((Bounds)(ref result)).Encapsulate(val7);
				}
			}
		}
		return result;
	}
}

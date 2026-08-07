using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Jobs;

namespace Facepunch.Extend;

public static class TransformEx
{
	public static class Unsafe
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private unsafe static TransformAccess* ExtractTransformAccess(TransformHandle handle)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			if (((TransformHandle)(ref handle)).Equals(TransformHandle.None))
			{
				throw new ArgumentNullException("handle", "A None/default handle has been passed in, it can't be used for transform access!");
			}
			IntPtr* ptr = (IntPtr*)UnsafeUtility.AddressOf<TransformHandle>(ref handle);
			return (TransformAccess*)(void*)(*ptr);
		}

		public unsafe static Vector3 GetLocalPosMT(in TransformHandle handle)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			return ((TransformAccess)ExtractTransformAccess(handle)).localPosition;
		}

		public unsafe static Quaternion GetLocalRotMT(in TransformHandle handle)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			return ((TransformAccess)ExtractTransformAccess(handle)).localRotation;
		}

		public unsafe static Vector3 GetLocalScaleMT(in TransformHandle handle)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			return ((TransformAccess)ExtractTransformAccess(handle)).localScale;
		}

		public unsafe static Vector3 GetPosMT(in TransformHandle handle)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			return ((TransformAccess)ExtractTransformAccess(handle)).position;
		}

		public unsafe static Quaternion GetRotMT(in TransformHandle handle)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			return ((TransformAccess)ExtractTransformAccess(handle)).rotation;
		}
	}

	private static PointerEventData _pointerEvent;

	public static Transform FindChildRecursive(this Transform transform, string name)
	{
		Transform val = transform.Find(name);
		for (int i = 0; i < transform.childCount; i++)
		{
			if (!((Object)(object)val == (Object)null))
			{
				break;
			}
			val = transform.GetChild(i).FindChildRecursive(name);
		}
		return val;
	}

	public static T GetOrAddComponent<T>(this Transform transform) where T : Component
	{
		T val = ((Component)transform).GetComponent<T>();
		if ((Object)(object)val == (Object)null)
		{
			val = ((Component)transform).gameObject.AddComponent<T>();
		}
		return val;
	}

	public static void DestroyAllChildren(this Transform transform, bool immediate = false)
	{
		for (int num = transform.childCount; num > 0; num--)
		{
			Transform child = transform.GetChild(num - 1);
			if (!((Component)child).CompareTag("persist"))
			{
				if (immediate)
				{
					Object.DestroyImmediate((Object)(object)((Component)child).gameObject);
				}
				else
				{
					Object.Destroy((Object)(object)((Component)child).gameObject);
				}
			}
		}
	}

	public static float AngleToPos(this Transform transform, Vector3 targetPos)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		float y = transform.eulerAngles.y;
		Vector3 val = targetPos - transform.position;
		float num = Mathf.Atan2(val.x, val.z) * 57.29578f - y;
		if (num > 180f)
		{
			num -= 360f;
		}
		else if (num < -180f)
		{
			num += 360f;
		}
		return num;
	}

	public static int GetDepth(this Transform transform)
	{
		int num = 0;
		Transform parent = transform.parent;
		while ((Object)(object)parent != (Object)null)
		{
			num++;
			parent = parent.parent;
		}
		return num;
	}

	public static bool ClickedInsideTransformOrChild(this Transform t, int? mouseButton = null)
	{
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Expected O, but got Unknown
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		if (mouseButton.HasValue && mouseButton.Value switch
		{
			0 => Mouse.current.leftButton.isPressed ? 1 : 0, 
			1 => Mouse.current.rightButton.isPressed ? 1 : 0, 
			2 => Mouse.current.middleButton.isPressed ? 1 : 0, 
			_ => 0, 
		} == 0)
		{
			return false;
		}
		EventSystem current = EventSystem.current;
		if ((Object)(object)current == (Object)null)
		{
			return false;
		}
		if (_pointerEvent == null)
		{
			_pointerEvent = new PointerEventData(current);
		}
		_pointerEvent.position = ((InputControl<Vector2>)(object)((Pointer)Mouse.current).position).ReadValue();
		List<RaycastResult> list = Pool.Get<List<RaycastResult>>();
		EventSystem.current.RaycastAll(_pointerEvent, list);
		foreach (RaycastResult item in list)
		{
			RaycastResult current2 = item;
			if (((RaycastResult)(ref current2)).gameObject.transform.IsChildOf(t))
			{
				Pool.FreeUnmanaged<RaycastResult>(ref list);
				return true;
			}
		}
		Pool.FreeUnmanaged<RaycastResult>(ref list);
		return false;
	}
}

using System;
using System.Runtime.CompilerServices;
using ConVar;
using UnityEngine;

[DisallowMultipleComponent]
public class CameraUpdateHook : MonoBehaviour
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static CameraCallback _003C_003E9__5_0;

		public static CameraCallback _003C_003E9__5_1;

		public static CameraCallback _003C_003E9__5_2;

		internal void _003CAwake_003Eb__5_0(Camera args)
		{
			Camera mainCamera = MainCamera.mainCamera;
			LastFrameFOV = ((mainCamera != null) ? mainCamera.fieldOfView : Graphics.fov);
			PreRender?.Invoke();
		}

		internal void _003CAwake_003Eb__5_1(Camera args)
		{
			PostRender?.Invoke();
		}

		internal void _003CAwake_003Eb__5_2(Camera args)
		{
			PreCull?.Invoke();
		}
	}

	public static Action PreCull;

	public static Action PreRender;

	public static Action PostRender;

	public static Action RustCamera_PreRender;

	public static float LastFrameFOV = Graphics.fov;

	private void Awake()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected Obj, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected Obj, but got Unknown
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Expected Obj, but got Unknown
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected Obj, but got Unknown
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Expected Obj, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Expected Obj, but got Unknown
		CameraUpdateHook[] components = ((Component)this).GetComponents<CameraUpdateHook>();
		foreach (CameraUpdateHook cameraUpdateHook in components)
		{
			if ((Object)(object)cameraUpdateHook != (Object)(object)this)
			{
				Object.DestroyImmediate((Object)(object)cameraUpdateHook);
			}
		}
		CameraCallback onPreRender = Camera.onPreRender;
		CameraCallback val = _003C_003Ec._003C_003E9__5_0;
		if (val == null)
		{
			CameraCallback val2 = (Camera args) =>
			{
				Camera mainCamera = MainCamera.mainCamera;
				LastFrameFOV = ((mainCamera != null) ? mainCamera.fieldOfView : Graphics.fov);
				PreRender?.Invoke();
			};
			_003C_003Ec._003C_003E9__5_0 = val2;
			val = val2;
		}
		Camera.onPreRender = (CameraCallback)Delegate.Combine((Delegate?)(object)onPreRender, (Delegate?)(object)val);
		CameraCallback onPostRender = Camera.onPostRender;
		CameraCallback val3 = _003C_003Ec._003C_003E9__5_1;
		if (val3 == null)
		{
			CameraCallback val4 = (Camera args) =>
			{
				PostRender?.Invoke();
			};
			_003C_003Ec._003C_003E9__5_1 = val4;
			val3 = val4;
		}
		Camera.onPostRender = (CameraCallback)Delegate.Combine((Delegate?)(object)onPostRender, (Delegate?)(object)val3);
		CameraCallback onPreCull = Camera.onPreCull;
		CameraCallback val5 = _003C_003Ec._003C_003E9__5_2;
		if (val5 == null)
		{
			CameraCallback val6 = (Camera args) =>
			{
				PreCull?.Invoke();
			};
			_003C_003Ec._003C_003E9__5_2 = val6;
			val5 = val6;
		}
		Camera.onPreCull = (CameraCallback)Delegate.Combine((Delegate?)(object)onPreCull, (Delegate?)(object)val5);
	}
}

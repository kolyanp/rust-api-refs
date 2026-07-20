using Facepunch;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class RagdollEditor : SingletonComponent<RagdollEditor>
{
	private Vector3 view;

	private Rigidbody grabbedRigid;

	private Vector3 grabPos;

	private Vector3 grabOffset;

	private void OnGUI()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		GUI.Box(new Rect((float)Screen.width * 0.5f - 2f, (float)Screen.height * 0.5f - 2f, 4f, 4f), "");
	}

	protected override void Awake()
	{
		base.Awake();
	}

	private void Update()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		Camera.main.fieldOfView = 75f;
		if (Mouse.current.rightButton.isPressed)
		{
			Vector2 val = MouseDelta.Read();
			view.y += val.x * 3f;
			view.x -= val.y * 3f;
			Cursor.lockState = (CursorLockMode)1;
			Cursor.visible = false;
		}
		else
		{
			Cursor.lockState = (CursorLockMode)0;
			Cursor.visible = true;
		}
		((Component)Camera.main).transform.rotation = Quaternion.Euler(view);
		Vector3 val2 = Vector3.zero;
		if (((ButtonControl)Keyboard.current[(Key)37]).isPressed)
		{
			val2 += Vector3.forward;
		}
		if (((ButtonControl)Keyboard.current[(Key)33]).isPressed)
		{
			val2 += Vector3.back;
		}
		if (((ButtonControl)Keyboard.current[(Key)15]).isPressed)
		{
			val2 += Vector3.left;
		}
		if (((ButtonControl)Keyboard.current[(Key)18]).isPressed)
		{
			val2 += Vector3.right;
		}
		Transform transform = ((Component)Camera.main).transform;
		transform.position += ((Component)this).transform.rotation * val2 * 0.05f;
		if (Mouse.current.leftButton.wasPressedThisFrame)
		{
			StartGrab();
		}
		if (Mouse.current.leftButton.wasReleasedThisFrame)
		{
			StopGrab();
		}
	}

	private void FixedUpdate()
	{
		if (Mouse.current.leftButton.isPressed)
		{
			UpdateGrab();
		}
	}

	private void StartGrab()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit val = default(RaycastHit);
		if (Physics.Raycast(((Component)this).transform.position, ((Component)this).transform.forward, ref val, 100f))
		{
			grabbedRigid = ((Component)((RaycastHit)(ref val)).collider).GetComponent<Rigidbody>();
			if (!((Object)(object)grabbedRigid == (Object)null))
			{
				Matrix4x4 worldToLocalMatrix = ((Component)grabbedRigid).transform.worldToLocalMatrix;
				grabPos = ((Matrix4x4)(ref worldToLocalMatrix)).MultiplyPoint(((RaycastHit)(ref val)).point);
				worldToLocalMatrix = ((Component)this).transform.worldToLocalMatrix;
				grabOffset = ((Matrix4x4)(ref worldToLocalMatrix)).MultiplyPoint(((RaycastHit)(ref val)).point);
			}
		}
	}

	private void UpdateGrab()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)grabbedRigid == (Object)null))
		{
			Vector3 val = ((Component)this).transform.TransformPoint(grabOffset);
			Vector3 val2 = ((Component)grabbedRigid).transform.TransformPoint(grabPos);
			Vector3 val3 = val - val2;
			grabbedRigid.AddForceAtPosition(val3 * 100f * grabbedRigid.mass, val2, (ForceMode)0);
		}
	}

	private void StopGrab()
	{
		grabbedRigid = null;
	}
}

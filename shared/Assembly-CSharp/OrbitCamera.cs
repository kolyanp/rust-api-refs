using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class OrbitCamera : MonoBehaviour
{
	[Header("References")]
	public Transform yaw;

	public Transform pitch;

	public Transform cam;

	[Header("Orbit Settings")]
	public float yawSpeed = 150f;

	public float pitchSpeed = 150f;

	[Header("Zoom Settings")]
	public float zoomSpeed = 0.03f;

	public float fastZoomMultiplier = 5f;

	public float minZoom = 0.25f;

	public float maxZoom = 10f;

	[Header("Lock Targets")]
	public Vector3[] lockPositions;

	private int _currentLockIndex;

	private float _currentZoom;

	protected void Start()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		_currentZoom = 0f - cam.localPosition.z;
		((Component)this).transform.position = lockPositions[_currentLockIndex];
	}

	protected void Update()
	{
		HandleOrbit();
		HandleZoom();
	}

	private void HandleOrbit()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		Mouse current = Mouse.current;
		if (current != null && current.rightButton.isPressed)
		{
			Vector2 val = ((InputControl<Vector2>)(object)((Pointer)current).delta).ReadValue() * 0.1f;
			yaw.Rotate(0f, val.x * yawSpeed * Time.deltaTime, 0f);
			float num = pitch.localEulerAngles.x;
			if (num > 180f)
			{
				num -= 360f;
			}
			num -= val.y * pitchSpeed * Time.deltaTime;
			pitch.localEulerAngles = new Vector3(num, 0f, 0f);
		}
	}

	private void HandleZoom()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		Mouse current = Mouse.current;
		if (current != null)
		{
			float y = ((InputControl<Vector2>)(object)current.scroll).ReadValue().y;
			float num = zoomSpeed;
			Keyboard current2 = Keyboard.current;
			if (current2 != null && (((ButtonControl)current2.leftShiftKey).isPressed || ((ButtonControl)current2.rightShiftKey).isPressed))
			{
				num *= fastZoomMultiplier;
			}
			_currentZoom -= y * num;
			_currentZoom = Mathf.Clamp(_currentZoom, minZoom, maxZoom);
			cam.localPosition = new Vector3(0f, 0f, 0f - _currentZoom);
		}
	}

	private void ToggleLockPosition()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		_currentLockIndex = (_currentLockIndex + 1) % lockPositions.Length;
		((Component)this).transform.position = lockPositions[_currentLockIndex];
	}

	public void SetLockPos(int i)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		_currentLockIndex = Mathf.Clamp(i, 0, lockPositions.Length - 1);
		((Component)this).transform.position = lockPositions[_currentLockIndex];
	}
}

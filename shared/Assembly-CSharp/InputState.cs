using UnityEngine;

public class InputState
{
	public InputMessage current = new InputMessage
	{
		ShouldPool = false
	};

	public InputMessage previous = new InputMessage
	{
		ShouldPool = false
	};

	private int SwallowedButtons;

	public bool IsDown(BUTTON btn)
	{
		if (current == null)
		{
			return false;
		}
		if (((uint)SwallowedButtons & (uint)btn) == (uint)btn)
		{
			return false;
		}
		return ((uint)current.buttons & (uint)btn) == (uint)btn;
	}

	public bool WasDown(BUTTON btn)
	{
		if (previous == null)
		{
			return false;
		}
		return ((uint)previous.buttons & (uint)btn) == (uint)btn;
	}

	public bool IsAnyDown()
	{
		if (current == null)
		{
			return false;
		}
		return (float)(current.buttons & ~SwallowedButtons) > 0f;
	}

	public bool WasJustPressed(BUTTON btn)
	{
		if (IsDown(btn))
		{
			return !WasDown(btn);
		}
		return false;
	}

	public bool WasJustReleased(BUTTON btn)
	{
		if (!IsDown(btn))
		{
			return WasDown(btn);
		}
		return false;
	}

	public void SwallowButton(BUTTON btn)
	{
		if (current != null)
		{
			SwallowedButtons |= (int)btn;
		}
	}

	public Quaternion AimAngle()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		if (current == null)
		{
			return Quaternion.identity;
		}
		return Quaternion.Euler(current.aimAngles);
	}

	public Vector3 MouseDelta()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		if (current == null)
		{
			return Vector3.zero;
		}
		return current.mouseDelta;
	}

	public float Steer()
	{
		float num = ((current != null) ? SanitiseAnalog(current.steer) : 0f);
		if (num != 0f)
		{
			return num;
		}
		if (IsDown(BUTTON.LEFT))
		{
			return -1f;
		}
		if (IsDown(BUTTON.RIGHT))
		{
			return 1f;
		}
		return 0f;
	}

	public float SteerSummed()
	{
		float num = ((current != null) ? SanitiseAnalog(current.steer) : 0f);
		if (num != 0f)
		{
			return num;
		}
		return (IsDown(BUTTON.RIGHT) ? 1f : 0f) - (IsDown(BUTTON.LEFT) ? 1f : 0f);
	}

	public float Throttle()
	{
		float num = ((current != null) ? SanitiseAnalog(current.throttle) : 0f);
		if (num != 0f)
		{
			return num;
		}
		if (IsDown(BUTTON.FORWARD))
		{
			return 1f;
		}
		if (IsDown(BUTTON.BACKWARD))
		{
			return -1f;
		}
		return 0f;
	}

	private static float SanitiseAnalog(float value)
	{
		if (!float.IsFinite(value))
		{
			return 0f;
		}
		return Mathf.Clamp(value, -1f, 1f);
	}

	public void Flip(InputMessage newcurrent)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		SwallowedButtons = 0;
		previous.aimAngles = current.aimAngles;
		previous.buttons = current.buttons;
		previous.mouseDelta = current.mouseDelta;
		previous.steer = current.steer;
		previous.throttle = current.throttle;
		current.aimAngles = newcurrent.aimAngles;
		current.buttons = newcurrent.buttons;
		current.mouseDelta = newcurrent.mouseDelta;
		current.steer = newcurrent.steer;
		current.throttle = newcurrent.throttle;
	}

	public void Clear()
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		current.buttons = 0;
		previous.buttons = 0;
		current.steer = 0f;
		previous.steer = 0f;
		current.throttle = 0f;
		previous.throttle = 0f;
		SetMouseDelta(Vector3.zero);
		SwallowedButtons = 0;
	}

	public void SetMouseDelta(Vector3 d)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		previous.mouseDelta = d;
		current.mouseDelta = d;
	}

	public InputState()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected Obj, but got Unknown
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected Obj, but got Unknown
	}
}

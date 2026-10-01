using UnityEngine;

namespace FIMSpace.GroundFitter;

public class FGroundFitter_Input : FGroundFitter_InputBase
{
	protected virtual void Update()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		if (Input.GetKeyDown((KeyCode)32))
		{
			TriggerJump();
		}
		Vector3 zero = Vector3.zero;
		if (Input.GetKey((KeyCode)119) || Input.GetKey((KeyCode)97) || Input.GetKey((KeyCode)115) || Input.GetKey((KeyCode)100))
		{
			if (Input.GetKey((KeyCode)304))
			{
				Sprint = true;
			}
			else
			{
				Sprint = false;
			}
			if (Input.GetKey((KeyCode)119))
			{
				zero.z++;
			}
			if (Input.GetKey((KeyCode)97))
			{
				zero.x--;
			}
			if (Input.GetKey((KeyCode)100))
			{
				zero.x++;
			}
			if (Input.GetKey((KeyCode)115))
			{
				zero.z--;
			}
			zero.Normalize();
			Quaternion val = Quaternion.LookRotation(zero);
			RotationOffset = val.eulerAngles.y;
			MoveVector = Vector3.forward;
		}
		else
		{
			Sprint = false;
			MoveVector = Vector3.zero;
		}
		if (Input.GetKey((KeyCode)120))
		{
			MoveVector -= Vector3.forward;
		}
		if (Input.GetKey((KeyCode)113))
		{
			MoveVector += Vector3.left;
		}
		if (Input.GetKey((KeyCode)101))
		{
			MoveVector += Vector3.right;
		}
		Vector3 moveVector = MoveVector;
		moveVector.Normalize();
		controller.Sprint = Sprint;
		controller.MoveVector = MoveVector;
		controller.RotationOffset = RotationOffset;
	}
}

using System.Threading.Tasks;
using CompanionServer.Cameras;
using Facepunch;
using ProtoBuf;
using UnityEngine;

namespace CompanionServer.Handlers;

public class CameraInput : BaseHandler<AppCameraInput>
{
	protected override double TokenCost => 0.01;

	public override ValueTask Execute()
	{
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		if (!CameraRenderer.enabled)
		{
			SendError("not_enabled");
			return default(ValueTask);
		}
		if (base.Client.CurrentCamera == null || !base.Client.IsControllingCamera)
		{
			SendError("no_camera");
			return default(ValueTask);
		}
		InputState inputState = base.Client.InputState;
		if (inputState == null)
		{
			inputState = new InputState();
			base.Client.InputState = inputState;
		}
		InputMessage val = Pool.Get<InputMessage>();
		val.buttons = base.Proto.buttons;
		val.mouseDelta = Sanitize(Vector2.op_Implicit(base.Proto.mouseDelta));
		val.aimAngles = Vector3.zero;
		inputState.Flip(val);
		val.Dispose();
		val = null;
		base.Client.CurrentCamera.UserInput(inputState, new CameraViewerId(base.Client.ControllingSteamId, base.Client.ConnectionId));
		SendSuccess();
		return default(ValueTask);
	}

	private static Vector3 Sanitize(Vector3 value)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		return new Vector3(Sanitize(value.x), Sanitize(value.y), Sanitize(value.z));
	}

	private static float Sanitize(float value)
	{
		if (float.IsNaN(value) || float.IsInfinity(value))
		{
			return 0f;
		}
		return Mathf.Clamp(value, -100f, 100f);
	}
}

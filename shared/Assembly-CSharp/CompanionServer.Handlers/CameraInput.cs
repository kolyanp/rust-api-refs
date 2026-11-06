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
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		if (!CameraRenderer.enabled)
		{
			SendError("not_enabled");
			return default;
		}
		IRemoteControllable currentCamera = Client.CurrentCamera;
		if (currentCamera == null)
		{
			Client.EndViewing();
			SendError("no_camera");
			return default;
		}
		if (!Client.IsControllingCamera)
		{
			SendError("no_camera");
			return default;
		}
		InputState inputState = Client.InputState;
		if (inputState == null)
		{
			inputState = new InputState();
			Client.InputState = inputState;
		}
		InputMessage val = Pool.Get<InputMessage>();
		val.buttons = Proto.buttons;
		val.mouseDelta = Sanitize(Vector2.op_Implicit(Proto.mouseDelta));
		val.aimAngles = Vector3.zero;
		inputState.Flip(val);
		val.Dispose();
		val = null;
		currentCamera.UserInput(inputState, new CameraViewerId(Client.ControllingSteamId, Client.ConnectionId));
		SendSuccess();
		return default;
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

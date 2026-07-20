public static class CameraModeEx
{
	public static bool IsFirstPerson(this BasePlayer.CameraMode cameraMode)
	{
		if (cameraMode != BasePlayer.CameraMode.FirstPerson)
		{
			return cameraMode == BasePlayer.CameraMode.FirstPersonWithArms;
		}
		return true;
	}
}

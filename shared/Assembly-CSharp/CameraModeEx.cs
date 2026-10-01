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

	public static bool IsHeadMounted(this BasePlayer.CameraMode cameraMode)
	{
		if (!cameraMode.IsFirstPerson())
		{
			return cameraMode == BasePlayer.CameraMode.Eyes;
		}
		return true;
	}
}

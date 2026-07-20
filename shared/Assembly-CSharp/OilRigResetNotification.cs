using System;
using UnityEngine;

public class OilRigResetNotification : MonoBehaviour
{
	private enum OilRigType
	{
		Undefined,
		Small,
		Large
	}

	[SerializeField]
	private OilRigType oilRigType;

	public void OnPuzzleReset()
	{
		Server_HandlePuzzleReset();
	}

	public void Server_HandlePuzzleReset()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer.Server_SendWorldNotificationToAllActivePlayers(oilRigType switch
		{
			OilRigType.Undefined => WorldNotificationConfig.NotificationType.Undefined, 
			OilRigType.Small => WorldNotificationConfig.NotificationType.SmallOilRigReset, 
			OilRigType.Large => WorldNotificationConfig.NotificationType.LargeOilRigReset, 
			_ => throw new ArgumentOutOfRangeException(), 
		}, ((Component)this).transform.position);
	}
}

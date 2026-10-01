using ProtoBuf.Nexus;
using UnityEngine;

namespace Rust.Nexus.Handlers;

public class RespawnAtBagHandler : BaseNexusRequestHandler<SleepingBagRespawnRequest>
{
	protected override void Handle()
	{
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = BasePlayer.FindByID(Request.userId) ?? BasePlayer.FindSleeping(Request.userId);
		if ((Object)(object)basePlayer != (Object)null)
		{
			if (basePlayer.IsConnected)
			{
				basePlayer.Kick("You're apparently respawning from a another zone - contact developers!");
			}
			basePlayer.Kill();
		}
		BasePlayer basePlayer2 = SingletonComponent<ServerMgr>.Instance.SpawnNewPlayer(null);
		basePlayer2.userID = Request.userId;
		basePlayer2.UserIDString = Request.userId.ToString();
		basePlayer2.displayName = basePlayer2.UserIDString;
		basePlayer2.SetPlayerFlag(BasePlayer.PlayerFlags.LoadingAfterTransfer, b: true);
		basePlayer2.UpdateGender();
		if (!SleepingBag.TrySpawnPlayer(basePlayer2, Request.sleepingBagId, out var errorMessage))
		{
			basePlayer2.Kill();
			SendError(errorMessage);
		}
		if (basePlayer2.isMounted)
		{
			basePlayer2.DisableTransferProtection();
		}
		else if (!basePlayer2.isMounted)
		{
			basePlayer2.SetPlayerFlag(BasePlayer.PlayerFlags.LoadingAfterTransfer, b: false);
		}
		basePlayer2.LoadSecondaryData(Request.secondaryData);
		basePlayer2.LoadClanInfo();
	}
}

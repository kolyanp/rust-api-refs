using System;
using ConVar;
using Network;
using UnityEngine;
using UnityEngine.Assertions;

public class BowWeapon : ArrowWeapon
{
	private Action _updateFireFlagAction;

	private Action UpdateFireFlagAction => UpdateFireFlag;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("BowWeapon.OnRpcMessage"))
		{
			if (rpc == 4228048190u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - BowReload"));
				}
				using (TimeWarning.New("BowReload"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsActiveItem.Test(4228048190u, "BowReload", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg2 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							BowReload(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in BowReload");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void OnHeldChanged()
	{
		using (TimeWarning.New("BowWeapon.OnHeldChanged"))
		{
			base.OnHeldChanged();
			if (!base.isServer)
			{
				return;
			}
			if (IsDeployed())
			{
				InvokeRepeating(UpdateFireFlagAction, 0.1f, 0.1f);
				return;
			}
			CancelInvoke(UpdateFireFlagAction);
			if (!IsOnFire())
			{
				return;
			}
			using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope.Set(Flags.OnFire, b: false);
		}
	}

	private void UpdateFireFlag()
	{
		using (TimeWarning.New("BowWeapon.UpdateFireFlag"))
		{
			BasePlayer ownerPlayer = GetOwnerPlayer();
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				if (!ObjectEx.IsUnityNull(ownerPlayer))
				{
					if (!IsOnFire() && ownerPlayer.modelState.aiming && (Object)(object)primaryMagazine.ammoType == (Object)(object)ArrowItemDefinitions.FireArrowItemDef)
					{
						flagsUpdateScope.Set(Flags.OnFire, b: true);
					}
					else if (IsOnFire() && (!ownerPlayer.modelState.aiming || (Object)(object)primaryMagazine.ammoType != (Object)(object)ArrowItemDefinitions.FireArrowItemDef))
					{
						flagsUpdateScope.Set(Flags.OnFire, b: false);
					}
				}
				else
				{
					flagsUpdateScope.Set(Flags.OnFire, b: false);
				}
			}
			if (IsOnFire())
			{
				SingletonComponent<NpcFireManager>.Instance.Move(this);
			}
		}
	}

	[RPC_Server.IsActiveItem]
	[RPC_Server]
	private void BowReload(RPCMessage msg)
	{
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if ((Object)(object)ownerPlayer != (Object)null)
		{
			TryReloadMagazine(ownerPlayer.inventory);
		}
	}
}

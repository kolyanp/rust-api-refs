using System;
using ConVar;
using Network;
using UnityEngine;
using UnityEngine.Assertions;

public class VehicleModuleTaxi : VehicleModuleStorage
{
	[SerializeField]
	[Header("Taxi")]
	private SoundDefinition kickButtonSound;

	[SerializeField]
	private SphereCollider kickButtonCollider;

	[SerializeField]
	private float maxKickVelocity = 4f;

	private Vector3 KickButtonPos
	{
		get
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			return ((Component)kickButtonCollider).transform.position + ((Component)kickButtonCollider).transform.rotation * kickButtonCollider.center;
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("VehicleModuleTaxi.OnRpcMessage"))
		{
			if (rpc == 2714639811u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_KickPassengers"));
				}
				using (TimeWarning.New("RPC_KickPassengers"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(2714639811u, "RPC_KickPassengers", this, player, 3f))
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
							RPC_KickPassengers(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in RPC_KickPassengers");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	private bool CanKickPassengers(BasePlayer player)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (!base.IsOnAVehicle)
		{
			return false;
		}
		if (base.Vehicle.GetSpeed() > maxKickVelocity)
		{
			return false;
		}
		if ((Object)(object)player == (Object)null)
		{
			return false;
		}
		if (!base.Vehicle.PlayerIsMounted(player))
		{
			return false;
		}
		Vector3 val = KickButtonPos - ((Component)player).transform.position;
		if (Vector3.Dot(val, ((Component)player).transform.forward) < 0f)
		{
			return ((Vector3)(ref val)).sqrMagnitude < 4f;
		}
		return false;
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	public void RPC_KickPassengers(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!((Object)(object)player == (Object)null) && CanKickPassengers(player))
		{
			KickPassengers();
		}
	}

	private void KickPassengers()
	{
		if (!base.IsOnAVehicle)
		{
			return;
		}
		foreach (MountPointInfo mountPoint in mountPoints)
		{
			BaseMountable mountable = mountPoint.mountable;
			BasePlayer mounted = mountable.GetMounted();
			if ((Object)(object)mounted != (Object)null && mountable.HasValidDismountPosition(mounted))
			{
				mountable.AttemptDismount(mounted);
			}
		}
	}
}

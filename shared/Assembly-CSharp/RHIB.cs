using System;
using ConVar;
using Network;
using UnityEngine;
using UnityEngine.Assertions;

public class RHIB : MotorRowboat
{
	[Header("IK")]
	public bool useLeftHand = true;

	public Transform handLerpTarget;

	public Transform steeringWheelLeftHandTarget;

	public Transform steeringWheelRightHandTarget;

	public Transform throttleHandTarget;

	[Header("Throttle Animation")]
	public Transform throttleTransform;

	public bool usePositionAnimation = true;

	[Header("Throttle Animation - Position")]
	public Vector3 maxThrottle;

	public Vector3 noThrottle;

	public Vector3 minThrottle;

	[Header("Throttle Animation - Rotation")]
	public Vector3 maxThrottleRotation;

	public Vector3 noThrottleRotation;

	public Vector3 minThrottleRotation;

	[Header("Multi Propeller Animation")]
	public Transform[] multiPropellerRotate;

	[ServerVar(Help = "Population active on the server", ShowInAdminUI = true)]
	public static float rhibpopulation;

	private float targetGasPedal;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("RHIB.OnRpcMessage"))
		{
			if (rpc == 1382282393 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_Release"));
				}
				using (TimeWarning.New("Server_Release"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(1382282393u, "Server_Release", this, player, 6f))
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
							Server_Release(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in Server_Release");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	[RPC_Server.IsVisible(6f)]
	[RPC_Server]
	public void Server_Release(RPCMessage msg)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)GetParentEntity() == (Object)null))
		{
			SetParent(null, worldPositionStays: true, sendImmediate: true);
			SetToNonKinematic();
			buoyancy.Wake();
			rigidBody.centerOfMass = centerOfMass.localPosition + Vector3.down * 7f;
			Invoke(delegate
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				rigidBody.centerOfMass = centerOfMass.localPosition;
			}, 3f);
		}
	}

	public override void VehicleFixedUpdate()
	{
		using (TimeWarning.New("RHIB.VehicleFixedUpdate"))
		{
			gasPedal = Mathf.MoveTowards(gasPedal, targetGasPedal, Time.fixedDeltaTime * 1f);
			base.VehicleFixedUpdate();
		}
	}

	public override bool EngineOn()
	{
		return base.EngineOn();
	}

	public override void LightToggle(BasePlayer player)
	{
		base.LightToggle(player);
		BasePlayer driver = GetDriver();
		if ((Object)(object)driver == (Object)null || driver.userID.Get() != player.userID.Get())
		{
			return;
		}
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved5, !HasFlag(Flags.Reserved5));
	}

	public override void DriverInput(InputState inputState, BasePlayer player)
	{
		base.DriverInput(inputState, player);
		if (inputState.IsDown(BUTTON.FORWARD))
		{
			targetGasPedal = 1f;
		}
		else if (inputState.IsDown(BUTTON.BACKWARD))
		{
			targetGasPedal = -0.5f;
		}
		else
		{
			targetGasPedal = 0f;
		}
		if (inputState.IsDown(BUTTON.LEFT))
		{
			steering = 1f;
		}
		else if (inputState.IsDown(BUTTON.RIGHT))
		{
			steering = -1f;
		}
		else
		{
			steering = 0f;
		}
	}

	public void AddFuel(int amount)
	{
		fuelSystem.AddFuel(amount);
	}

	public void AdminKillNoLoot(bool killMountedNPCs = true)
	{
		StorageContainer storageContainer = storageUnitInstance.Get(base.isServer);
		if ((Object)(object)storageContainer != (Object)null)
		{
			storageContainer.dropsLoot = false;
		}
		if (killMountedNPCs)
		{
			for (int i = 0; i < mountPoints.Count; i++)
			{
				BaseMountable mountable = mountPoints[i].mountable;
				if (!((Object)(object)mountable == (Object)null))
				{
					BasePlayer mounted = mountable.GetMounted();
					if ((Object)(object)mounted != (Object)null && mounted is HumanNPC humanNPC && (Object)(object)humanNPC != (Object)null)
					{
						humanNPC.AdminKill();
					}
				}
			}
		}
		AdminKill();
	}

	public void KillMountedNPCs()
	{
	}
}

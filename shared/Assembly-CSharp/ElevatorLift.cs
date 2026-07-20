using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class ElevatorLift : BaseCombatEntity
{
	public GameObject DescendingHurtTrigger;

	public GameObject MovementCollider;

	public ElevatorButton[] Buttons;

	public Transform UpButtonPoint;

	public Transform DownButtonPoint;

	public Transform GoTopButtonPoint;

	public Transform GoBottomButtonPoint;

	public TriggerNotify VehicleTrigger;

	public GameObjectRef LiftArrivalScreenBounce;

	public SoundDefinition liftMovementLoopDef;

	public SoundDefinition liftMovementStartDef;

	public SoundDefinition liftMovementStopDef;

	public SoundDefinition liftMovementAccentSoundDef;

	public GameObjectRef liftButtonPressedEffect;

	public float movementAccentMinInterval = 0.75f;

	public float movementAccentMaxInterval = 3f;

	private Sound liftMovementLoopSound;

	private float nextMovementAccent;

	public Vector3 lastPosition;

	public List<BaseEntity> vehicleWhitelist;

	private EntityRef<Elevator> ownerElevator;

	public const Flags PressedUp = Flags.Reserved1;

	public const Flags PressedDown = Flags.Reserved2;

	public const Flags Express = Flags.Reserved6;

	public const Flags FlagCanMove = Flags.Reserved5;

	private HashSet<uint> vehiclePrefabWhitelist = new HashSet<uint>();

	protected Elevator owner => ownerElevator.Get(base.isServer);

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("ElevatorLift.OnRpcMessage"))
		{
			if (rpc == 4061236510u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_RaiseLowerFloor"));
				}
				using (TimeWarning.New("Server_RaiseLowerFloor"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(4061236510u, "Server_RaiseLowerFloor", this, player, 3f))
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
							Server_RaiseLowerFloor(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in Server_RaiseLowerFloor");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void Load(LoadInfo info)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		base.Load(info);
		if (info.msg.elevatorLift != null)
		{
			ownerElevator.uid = info.msg.elevatorLift.owner;
		}
	}

	public void SetOwnerElevator(Elevator e)
	{
		ownerElevator.Set(e);
	}

	public override void Save(SaveInfo info)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		info.msg.elevatorLift = Pool.Get<ElevatorLift>();
		if ((Object)(object)owner != (Object)null)
		{
			info.msg.elevatorLift.owner = ownerElevator.uid;
			info.msg.elevatorLift.topElevatorHeight = ((Component)owner).transform.position.y;
		}
	}

	public override void ServerInit()
	{
		base.ServerInit();
		FillVehicleWhitelist();
		ToggleHurtTrigger(state: false);
	}

	public void ToggleHurtTrigger(bool state)
	{
		if ((Object)(object)DescendingHurtTrigger != (Object)null)
		{
			DescendingHurtTrigger.SetActive(state);
		}
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void Server_RaiseLowerFloor(RPCMessage msg)
	{
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		int num = msg.read.Int32();
		bool flag = msg.read.Bool();
		if (Interface.CallHook("OnElevatorButtonPress", this, msg.player, num, flag) == null)
		{
			owner.Server_RaiseLowerElevator(num, flag, out var wantsMoveUp);
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(wantsMoveUp ? Flags.Reserved1 : Flags.Reserved2, b: true);
				flagsUpdateScope.Set(Flags.Reserved6, num == int.MinValue || num == int.MaxValue);
			}
			Invoke(ClearDirection, 0.7f);
			if (liftButtonPressedEffect.isValid)
			{
				Effect.server.Run(liftButtonPressedEffect.resourcePath, ((Component)this).transform.position, Vector3.up);
			}
		}
	}

	private void FillVehicleWhitelist()
	{
		foreach (BaseEntity item in vehicleWhitelist)
		{
			vehiclePrefabWhitelist.Add(item.prefabID);
		}
	}

	private void ClearDirection()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved1, b: false);
		flagsUpdateScope.Set(Flags.Reserved2, b: false);
		flagsUpdateScope.Set(Flags.Reserved6, b: false);
	}

	public override void Hurt(HitInfo info)
	{
		if ((Object)(object)owner != (Object)null)
		{
			owner.Hurt(info);
		}
	}

	public override void AdminKill()
	{
		if ((Object)(object)owner != (Object)null)
		{
			owner.AdminKill();
		}
		else
		{
			base.AdminKill();
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		ClearDirection();
	}

	public bool CanMove()
	{
		object obj = Interface.CallHook("CanElevatorLiftMove", this);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (VehicleTrigger.HasContents && VehicleTrigger.entityContents != null)
		{
			foreach (BaseEntity entityContent in VehicleTrigger.entityContents)
			{
				if (!vehiclePrefabWhitelist.Contains(entityContent.prefabID))
				{
					return false;
				}
			}
		}
		return true;
	}

	public virtual void NotifyNewFloor(int newFloor, int totalFloors)
	{
	}

	private void ToggleMovementCollider(bool state)
	{
		if ((Object)(object)MovementCollider != (Object)null)
		{
			MovementCollider.SetActive(state);
		}
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		ToggleMovementCollider((next & Flags.Busy) != Flags.Busy);
	}
}

using Facepunch;
using Oxide.Core;
using ProtoBuf;
using Rust;
using UnityEngine;

public class Elevator : IOEntity, IFlagNotify
{
	public enum Direction
	{
		Up,
		Down
	}

	public Transform LiftRoot;

	public GameObjectRef LiftEntityPrefab;

	public Transform IoEntitySpawnPoint;

	public GameObject FloorBlockerVolume;

	public float LiftSpeedPerMetre = 1f;

	public GameObject[] PoweredObjects;

	public MeshRenderer PoweredMesh;

	[ColorUsage(true, true)]
	public Color PoweredLightColour;

	[ColorUsage(true, true)]
	public Color UnpoweredLightColour;

	public float LiftMoveDelay;

	public float LiftEndMoveDelay = 1f;

	public float FloorHeightOverride;

	protected const Flags TopFloorFlag = Flags.Reserved1;

	public EntityRef<ElevatorLift> liftEntity;

	public int[] previousPowerAmount = new int[3];

	public virtual bool IsStatic => false;

	public int Floor { get; set; }

	public bool IsTop => HasFlag(Flags.Reserved1);

	public virtual float FloorHeight
	{
		get
		{
			if (!(FloorHeightOverride > 0f))
			{
				return 3f;
			}
			return FloorHeightOverride;
		}
	}

	public override void Load(LoadInfo info)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		base.Load(info);
		if (info.msg.elevator != null)
		{
			Floor = info.msg.elevator.floor;
			liftEntity.uid = info.msg.elevator.spawnedLift;
		}
		if ((Object)(object)FloorBlockerVolume != (Object)null)
		{
			FloorBlockerVolume.SetActive(Floor > 0);
		}
	}

	public override int ConsumptionAmount()
	{
		return 5;
	}

	public override void OnDeployed(BaseEntity parent, BasePlayer deployedBy, Item fromItem)
	{
		base.OnDeployed(parent, deployedBy, fromItem);
		Elevator elevatorInDirection = GetElevatorInDirection(Direction.Down);
		if ((Object)(object)elevatorInDirection != (Object)null)
		{
			using FlagsUpdateScope flagsUpdateScope = elevatorInDirection.StartSetFlags(FlagsUpdateMode.Local);
			flagsUpdateScope.Set(Flags.Reserved1, b: false);
			Floor = elevatorInDirection.Floor + 1;
		}
		using (FlagsUpdateScope flagsUpdateScope2 = StartSetFlags(FlagsUpdateMode.Local))
		{
			flagsUpdateScope2.Set(Flags.Reserved1, b: true);
		}
		UpdateChildEntities(isTop: true);
		SendNetworkUpdate();
		RefreshPowerStatus();
	}

	public virtual void CallElevator()
	{
		EntityLinkBroadcast(delegate(Elevator elevatorEnt)
		{
			if (elevatorEnt.IsTop && Interface.CallHook("OnElevatorCall", this, elevatorEnt) == null)
			{
				elevatorEnt.RequestMoveLiftTo(Floor, out var _, this);
			}
		}, (ConstructionSocket socket) => socket.socketType == ConstructionSocket.Type.Elevator);
	}

	public void Server_RaiseLowerElevator(int targetFloor, bool relative, out bool wantsMoveUp)
	{
		int num = LiftPositionToFloor();
		switch (targetFloor)
		{
		case int.MinValue:
			targetFloor = 0;
			break;
		case int.MaxValue:
			targetFloor = Floor;
			break;
		default:
			if (relative)
			{
				targetFloor = num + targetFloor;
			}
			break;
		}
		targetFloor = Mathf.Clamp(targetFloor, 0, Floor);
		wantsMoveUp = targetFloor > num;
		if (!IsBusy())
		{
			RequestMoveLiftTo(targetFloor, out var _, this);
		}
	}

	public bool RequestMoveLiftTo(int targetFloor, out float timeToTravel, Elevator fromElevator)
	{
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		timeToTravel = 0f;
		if (Interface.CallHook("OnElevatorMove", this, targetFloor) != null)
		{
			return false;
		}
		if (IsBusy())
		{
			return false;
		}
		if (!IsStatic && !IsPowered())
		{
			return false;
		}
		if (!IsValidFloor(targetFloor))
		{
			return false;
		}
		int num = LiftPositionToFloor();
		if (num == targetFloor)
		{
			OpenDoorsAtFloor(num);
			return false;
		}
		if (!liftEntity.IsValid(base.isServer))
		{
			return false;
		}
		ElevatorLift elevatorLift = liftEntity.Get(base.isServer);
		if (!elevatorLift.CanMove())
		{
			return false;
		}
		Vector3 worldSpaceFloorPosition = GetWorldSpaceFloorPosition(targetFloor);
		if (!GamePhysics.LineOfSight(((Component)elevatorLift).transform.position, worldSpaceFloorPosition, 2097152))
		{
			return false;
		}
		OnMoveBegin();
		timeToTravel = TimeToTravelDistance(Mathf.Abs(((Component)elevatorLift).transform.position.y - worldSpaceFloorPosition.y));
		LeanTween.moveY(((Component)elevatorLift).gameObject, worldSpaceFloorPosition.y, timeToTravel).delay = LiftMoveDelay;
		timeToTravel += LiftMoveDelay;
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Busy, b: true);
		}
		if (targetFloor < Floor)
		{
			elevatorLift.ToggleHurtTrigger(state: true);
		}
		using (FlagsUpdateScope flagsUpdateScope2 = elevatorLift.StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope2.Set(Flags.Busy, b: true);
		}
		Invoke(ClearBusy, timeToTravel + LiftEndMoveDelay);
		elevatorLift.NotifyNewFloor(targetFloor, Floor);
		EntityLinkBroadcast(delegate(Elevator elevatorEnt)
		{
			using FlagsUpdateScope flagsUpdateScope3 = elevatorEnt.StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope3.Set(Flags.Busy, b: true);
		}, (ConstructionSocket socket) => socket.socketType == ConstructionSocket.Type.Elevator);
		return true;
	}

	protected virtual void OpenLiftDoors()
	{
		NotifyLiftEntityDoorsOpen(state: true);
	}

	public virtual void OnMoveBegin()
	{
	}

	public float TimeToTravelDistance(float distance)
	{
		return distance / LiftSpeedPerMetre;
	}

	public virtual Vector3 GetWorldSpaceFloorPosition(int targetFloor)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		int num = Floor - targetFloor;
		Vector3 val = Vector3.up * ((float)num * FloorHeight);
		val.y -= 1f;
		return ((Component)this).transform.position - val;
	}

	public virtual void ClearBusy()
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Busy, b: false);
		}
		if (liftEntity.IsValid(base.isServer))
		{
			ElevatorLift elevatorLift = liftEntity.Get(base.isServer);
			elevatorLift.ToggleHurtTrigger(state: false);
			using FlagsUpdateScope flagsUpdateScope2 = elevatorLift.StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope2.Set(Flags.Busy, b: false);
		}
		EntityLinkBroadcast(delegate(Elevator elevatorEnt)
		{
			using FlagsUpdateScope flagsUpdateScope3 = elevatorEnt.StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope3.Set(Flags.Busy, b: false);
		}, (ConstructionSocket socket) => socket.socketType == ConstructionSocket.Type.Elevator);
	}

	public virtual bool IsValidFloor(int targetFloor)
	{
		if (targetFloor <= Floor)
		{
			return targetFloor >= 0;
		}
		return false;
	}

	public Elevator GetElevatorInDirection(Direction dir)
	{
		EntityLink entityLink = FindLink((dir == Direction.Down) ? "elevator/sockets/elevator-male" : "elevator/sockets/elevator-female");
		if (entityLink != null && !entityLink.IsEmpty())
		{
			BaseEntity owner = entityLink.connections[0].owner;
			if ((Object)(object)owner != (Object)null && owner.isServer && owner is Elevator elevator && (Object)(object)elevator != (Object)(object)this)
			{
				return elevator;
			}
		}
		return null;
	}

	public void UpdateChildEntities(bool isTop)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if (isTop)
		{
			if (!liftEntity.IsValid(base.isServer))
			{
				FindExistingLiftChild();
			}
			if (!liftEntity.IsValid(base.isServer))
			{
				ElevatorLift elevatorLift = GameManager.server.CreateEntity(LiftEntityPrefab.resourcePath, GetWorldSpaceFloorPosition(Floor), LiftRoot.rotation) as ElevatorLift;
				elevatorLift.SetOwnerElevator(this);
				elevatorLift.Spawn();
				liftEntity.Set(elevatorLift);
			}
			if (liftEntity.IsValid(base.isServer))
			{
				ElevatorLift elevatorLift2 = liftEntity.Get(base.isServer);
				if ((Object)(object)elevatorLift2.GetParentEntity() == (Object)(object)this)
				{
					elevatorLift2.SetParent(null, worldPositionStays: true);
				}
				elevatorLift2.SetOwnerElevator(this);
				using FlagsUpdateScope flagsUpdateScope = elevatorLift2.StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
				flagsUpdateScope.Set(Flags.Reserved5, IsPowered() || IsStatic);
			}
		}
		else if (liftEntity.IsValid(base.isServer))
		{
			liftEntity.Get(base.isServer).Kill();
			liftEntity.Set(null);
		}
	}

	public override void Save(SaveInfo info)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		if (info.msg.elevator == null)
		{
			info.msg.elevator = Pool.Get<Elevator>();
		}
		info.msg.elevator.floor = Floor;
		info.msg.elevator.spawnedLift = liftEntity.uid;
	}

	public override int DesiredPower(int inputIndex = 0)
	{
		return 5;
	}

	public int LiftPositionToFloor()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		if (!liftEntity.IsValid(base.isServer))
		{
			return 0;
		}
		Vector3 position = ((Component)liftEntity.Get(base.isServer)).transform.position;
		int result = -1;
		float num = float.MaxValue;
		for (int i = 0; i <= Floor; i++)
		{
			float num2 = Vector3.Distance(GetWorldSpaceFloorPosition(i), position);
			if (num2 < num)
			{
				num = num2;
				result = i;
			}
		}
		return result;
	}

	public override void DestroyShared()
	{
		Cleanup();
		base.DestroyShared();
	}

	private void Cleanup()
	{
		Elevator elevatorInDirection = GetElevatorInDirection(Direction.Down);
		if ((Object)(object)elevatorInDirection != (Object)null)
		{
			using FlagsUpdateScope flagsUpdateScope = elevatorInDirection.StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope.Set(Flags.Reserved1, b: true);
		}
		Elevator elevatorInDirection2 = GetElevatorInDirection(Direction.Up);
		if ((Object)(object)elevatorInDirection2 != (Object)null)
		{
			elevatorInDirection2.Kill(DestroyMode.Gib);
		}
		previousPowerAmount[2] = 0;
		RefreshPowerStatus();
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Busy, b: false);
		}
		UpdateChildEntities(IsTop);
	}

	public override void UpdateHasPower(int inputAmount, int inputSlot)
	{
		if (inputAmount > 0 && (inputSlot == 0 || inputSlot == 1) && previousPowerAmount[inputSlot] == 0)
		{
			CallElevator();
		}
		if (inputSlot == 2 && previousPowerAmount[inputSlot] != inputAmount)
		{
			previousPowerAmount[inputSlot] = inputAmount;
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved8, HasPowerInput());
			}
			RefreshPowerStatus();
		}
		previousPowerAmount[inputSlot] = inputAmount;
	}

	private void RefreshPowerStatus()
	{
		bool anyHasPower = false;
		EntityLinkBroadcast(delegate(Elevator elevatorEnt)
		{
			if (elevatorEnt.HasPowerInput())
			{
				anyHasPower = true;
			}
		}, (ConstructionSocket socket) => socket.socketType == ConstructionSocket.Type.Elevator);
		EntityLinkBroadcast(delegate(Elevator elevatorEnt)
		{
			using FlagsUpdateScope flagsUpdateScope = elevatorEnt.StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope.Set(Flags.Reserved8, anyHasPower);
		}, (ConstructionSocket socket) => socket.socketType == ConstructionSocket.Type.Elevator);
	}

	private bool HasPowerInput()
	{
		return previousPowerAmount[2] >= ConsumptionAmount();
	}

	private void OnPhysicsNeighbourChanged()
	{
		if (!IsStatic && (Object)(object)GetElevatorInDirection(Direction.Down) == (Object)null && !HasFloorSocketConnection())
		{
			Kill(DestroyMode.Gib);
		}
	}

	public bool HasFloorSocketConnection()
	{
		EntityLink entityLink = FindLink("elevator/sockets/block-male");
		if (entityLink != null && !entityLink.IsEmpty())
		{
			return true;
		}
		return false;
	}

	public void NotifyLiftEntityDoorsOpen(bool state)
	{
		if (!liftEntity.IsValid(base.isServer))
		{
			return;
		}
		foreach (BaseEntity child in liftEntity.Get(base.isServer).children)
		{
			if (child is Door door)
			{
				door.SetOpen(state);
			}
		}
	}

	protected virtual void OpenDoorsAtFloor(int floor)
	{
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		if ((Object)(object)liftEntity.Get(base.isServer) != (Object)null)
		{
			liftEntity.Get(base.isServer).Kill();
		}
	}

	public override void OnDied(HitInfo info)
	{
		base.OnDied(info);
		if ((Object)(object)liftEntity.Get(base.isServer) != (Object)null)
		{
			liftEntity.Get(base.isServer).Kill(DestroyMode.Gib);
		}
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		if (!Application.isLoading && base.isServer && (old & Flags.Reserved1) == Flags.Reserved1 != ((next & Flags.Reserved1) == Flags.Reserved1))
		{
			UpdateChildEntities((next & Flags.Reserved1) == Flags.Reserved1);
			SendNetworkUpdate();
		}
		if (base.isServer)
		{
			ElevatorLift elevatorLift = liftEntity.Get(base.isServer);
			if ((Object)(object)elevatorLift != (Object)null)
			{
				using FlagsUpdateScope flagsUpdateScope = elevatorLift.StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
				flagsUpdateScope.Set(Flags.Reserved5, IsPowered() || IsStatic);
			}
		}
		if ((old & Flags.Reserved1) == Flags.Reserved1 != ((next & Flags.Reserved1) == Flags.Reserved1) && (Object)(object)FloorBlockerVolume != (Object)null)
		{
			FloorBlockerVolume.SetActive((next & Flags.Reserved1) == Flags.Reserved1);
		}
		OnFlagToggled((next & Flags.Reserved8) == Flags.Reserved8);
	}

	public void FindExistingLiftChild()
	{
		foreach (BaseEntity child in children)
		{
			if (child is ElevatorLift entity)
			{
				liftEntity.Set(entity);
				break;
			}
		}
	}

	public void OnFlagToggled(bool state)
	{
	}
}

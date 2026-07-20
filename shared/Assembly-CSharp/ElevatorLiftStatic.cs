using Rust;
using UnityEngine;

public class ElevatorLiftStatic : ElevatorLift
{
	public GameObjectRef ElevatorDoorRef;

	public Transform ElevatorDoorLocation;

	public bool BlockPerFloorMovement;

	private const Flags CanGoUp = Flags.Reserved3;

	private const Flags CanGoDown = Flags.Reserved4;

	public override void ServerInit()
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		base.ServerInit();
		if (ElevatorDoorRef.isValid && (Object)(object)ElevatorDoorLocation != (Object)null && !Application.isLoadingSave)
		{
			foreach (BaseEntity child in children)
			{
				if (child is Door)
				{
					return;
				}
			}
			BaseEntity baseEntity = GameManager.server.CreateEntity(ElevatorDoorRef.resourcePath, ElevatorDoorLocation.localPosition, ElevatorDoorLocation.localRotation);
			baseEntity.SetParent(this);
			baseEntity.Spawn();
		}
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved3, b: false);
		flagsUpdateScope.Set(Flags.Reserved4, b: true);
	}

	public override void NotifyNewFloor(int newFloor, int totalFloors)
	{
		base.NotifyNewFloor(newFloor, totalFloors);
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved3, newFloor < totalFloors);
		flagsUpdateScope.Set(Flags.Reserved4, newFloor > 0);
	}
}

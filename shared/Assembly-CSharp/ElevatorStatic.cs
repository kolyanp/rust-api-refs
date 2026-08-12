using System.Collections.Generic;
using Facepunch;
using Rust;
using UnityEngine;

public class ElevatorStatic : Elevator
{
	public enum ElevatorFloorNumber
	{
		None,
		Basement,
		Lobby,
		Floor2,
		Floor3,
		Floor4,
		Floor5,
		Floor6,
		Floor7,
		Floor8,
		Penthouse,
		Floor0,
		Floor1,
		FloorMinus1
	}

	public bool StaticTop;

	public const Flags LiftRecentlyArrived = Flags.Reserved3;

	public List<EntityRef<ElevatorStatic>> clientFloorPositions = new List<EntityRef<ElevatorStatic>>();

	public List<ElevatorStatic> floorPositions = new List<ElevatorStatic>();

	public ElevatorStatic ownerElevator;

	public ElevatorFloorNumber FloorNumberDisplay;

	public static Phrase Phrase_Basement;

	public static Phrase Phrase_Elevator;

	public static Phrase Phrase_Lobby;

	public static Phrase Phrase_Penthouse;

	public static Phrase Phrase_GroundFloor;

	public static Phrase Phrase_Floor1;

	public static Phrase Phrase_Floor2;

	public static Phrase Phrase_Floor3;

	public static Phrase Phrase_Floor4;

	public static Phrase Phrase_Floor5;

	public static Phrase Phrase_Floor6;

	public static Phrase Phrase_Floor7;

	public static Phrase Phrase_Floor8;

	public static Dictionary<ElevatorFloorNumber, (string icon, Phrase phrase)> IconLookup;

	public override bool IsStatic => true;

	public IReadOnlyList<ElevatorStatic> FloorPositions => floorPositions;

	public ElevatorStatic TopElevator
	{
		get
		{
			if (!StaticTop)
			{
				return ownerElevator;
			}
			return this;
		}
	}

	public override void Spawn()
	{
		base.Spawn();
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved8, b: true);
			flagsUpdateScope.Set(Flags.Reserved1, StaticTop);
		}
		if (!Application.isLoadingSave)
		{
			UpdateFloorPositions();
		}
	}

	private void UpdateFloorPositions()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		if (!base.IsTop)
		{
			return;
		}
		floorPositions.Clear();
		List<RaycastHit> list = Pool.Get<List<RaycastHit>>();
		GamePhysics.TraceAll(new Ray(((Component)this).transform.position, -Vector3.up), 0f, list, 200f, 262144, (QueryTriggerInteraction)2);
		foreach (RaycastHit item in list)
		{
			RaycastHit current = item;
			if ((Object)(object)((RaycastHit)(ref current)).transform.parent != (Object)null)
			{
				ElevatorStatic component = ((Component)((RaycastHit)(ref current)).transform.parent).GetComponent<ElevatorStatic>();
				if (!((Object)(object)component == (Object)null) && !((Object)(object)component == (Object)(object)this) && !component.isClient && !component.IsDestroyed)
				{
					floorPositions.Add(component);
				}
			}
		}
		Pool.FreeUnmanaged<RaycastHit>(ref list);
		floorPositions.Reverse();
		base.Floor = floorPositions.Count;
		for (int i = 0; i < floorPositions.Count; i++)
		{
			floorPositions[i].SetFloorDetails(i, this);
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		Invoke(UpdateFloorPositions, 1f);
	}

	public override void PostMapEntitySpawn()
	{
		base.PostMapEntitySpawn();
		UpdateChildEntities(base.IsTop);
	}

	public override bool IsValidFloor(int targetFloor)
	{
		if (targetFloor >= 0)
		{
			return targetFloor <= base.Floor;
		}
		return false;
	}

	public override Vector3 GetWorldSpaceFloorPosition(int targetFloor)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (targetFloor == base.Floor)
		{
			return ((Component)this).transform.position + Vector3.up * 1f;
		}
		Vector3 position = ((Component)this).transform.position;
		position.y = ((Component)floorPositions[targetFloor]).transform.position.y + 1f;
		return position;
	}

	public void SetFloorDetails(int floor, ElevatorStatic owner)
	{
		ownerElevator = owner;
		base.Floor = floor;
	}

	public override void CallElevator()
	{
		if ((Object)(object)ownerElevator != (Object)null)
		{
			ownerElevator.RequestMoveLiftTo(base.Floor, out var _, this);
		}
		else if (base.IsTop)
		{
			RequestMoveLiftTo(base.Floor, out var _, this);
		}
	}

	public ElevatorStatic ElevatorAtFloor(int floor)
	{
		if (floor == base.Floor)
		{
			return this;
		}
		if (floor >= 0 && floor < floorPositions.Count)
		{
			return floorPositions[floor];
		}
		return null;
	}

	protected override void OpenDoorsAtFloor(int floor)
	{
		base.OpenDoorsAtFloor(floor);
		if (floor == floorPositions.Count)
		{
			OpenLiftDoors();
		}
		else
		{
			floorPositions[floor].OpenLiftDoors();
		}
	}

	public override void OnMoveBegin()
	{
		base.OnMoveBegin();
		ElevatorStatic elevatorStatic = ElevatorAtFloor(LiftPositionToFloor());
		if ((Object)(object)elevatorStatic != (Object)null)
		{
			elevatorStatic.OnLiftLeavingFloor();
		}
		NotifyLiftEntityDoorsOpen(state: false);
	}

	public void OnLiftLeavingFloor()
	{
		ClearPowerOutput();
		if (IsInvoking(ClearPowerOutput))
		{
			CancelInvoke(ClearPowerOutput);
		}
	}

	public override void ClearBusy()
	{
		base.ClearBusy();
		ElevatorStatic elevatorStatic = ElevatorAtFloor(LiftPositionToFloor());
		if ((Object)(object)elevatorStatic != (Object)null)
		{
			elevatorStatic.OnLiftArrivedAtFloor();
		}
		NotifyLiftEntityDoorsOpen(state: true);
	}

	protected override void OpenLiftDoors()
	{
		base.OpenLiftDoors();
		OnLiftArrivedAtFloor();
	}

	public void OnLiftArrivedAtFloor()
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved3, b: true);
		}
		MarkDirty();
		Invoke(ClearPowerOutput, 10f);
	}

	public void ClearPowerOutput()
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved3, b: false);
		}
		MarkDirty();
	}

	public override int GetPassthroughAmount(int outputSlot = 0)
	{
		if (!HasFlag(Flags.Reserved3))
		{
			return 0;
		}
		return 1;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.fromDisk)
		{
			using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope.Set(Flags.Reserved3, b: false);
		}
		FloorNumberDisplay = (ElevatorFloorNumber)info.msg.elevator.floorDisplay;
	}

	public override void Save(SaveInfo info)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		if (base.IsTop)
		{
			info.msg.elevator.floorList = Pool.Get<List<NetworkableId>>();
			foreach (ElevatorStatic floorPosition in floorPositions)
			{
				info.msg.elevator.floorList.Add(floorPosition.net.ID);
			}
		}
		info.msg.elevator.floorDisplay = (int)FloorNumberDisplay;
	}

	static ElevatorStatic()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected O, but got Unknown
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Expected O, but got Unknown
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Expected O, but got Unknown
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Expected O, but got Unknown
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Expected O, but got Unknown
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Expected O, but got Unknown
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Expected O, but got Unknown
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Expected O, but got Unknown
		Phrase_Basement = new Phrase("elevator_floor_option_basement", "Basement");
		Phrase_Elevator = new Phrase("elevator_floor_option_elevator", "Elevator");
		Phrase_Lobby = new Phrase("elevator_floor_option_lobby", "Lobby");
		Phrase_Penthouse = new Phrase("elevator_floor_option_penthouse", "Penthouse");
		Phrase_GroundFloor = new Phrase("elevator_floor_option_ground", "Ground Floor");
		Phrase_Floor1 = new Phrase("elevator_floor_option_1", "Floor 1");
		Phrase_Floor2 = new Phrase("elevator_floor_option_2", "Floor 2");
		Phrase_Floor3 = new Phrase("elevator_floor_option_3", "Floor 3");
		Phrase_Floor4 = new Phrase("elevator_floor_option_4", "Floor 4");
		Phrase_Floor5 = new Phrase("elevator_floor_option_5", "Floor 5");
		Phrase_Floor6 = new Phrase("elevator_floor_option_6", "Floor 6");
		Phrase_Floor7 = new Phrase("elevator_floor_option_7", "Floor 7");
		Phrase_Floor8 = new Phrase("elevator_floor_option_8", "Floor 8");
		IconLookup = new Dictionary<ElevatorFloorNumber, (string, Phrase)>
		{
			{
				ElevatorFloorNumber.None,
				("elevator", Phrase_Elevator)
			},
			{
				ElevatorFloorNumber.Basement,
				("elevator_b", Phrase_Basement)
			},
			{
				ElevatorFloorNumber.Lobby,
				("elevator_l", Phrase_Lobby)
			},
			{
				ElevatorFloorNumber.Penthouse,
				("elevator_p", Phrase_Penthouse)
			},
			{
				ElevatorFloorNumber.Floor2,
				("elevator_2", Phrase_Floor2)
			},
			{
				ElevatorFloorNumber.Floor3,
				("elevator_3", Phrase_Floor3)
			},
			{
				ElevatorFloorNumber.Floor4,
				("elevator_4", Phrase_Floor4)
			},
			{
				ElevatorFloorNumber.Floor5,
				("elevator_5", Phrase_Floor5)
			},
			{
				ElevatorFloorNumber.Floor6,
				("elevator_6", Phrase_Floor6)
			},
			{
				ElevatorFloorNumber.Floor7,
				("elevator_7", Phrase_Floor7)
			},
			{
				ElevatorFloorNumber.Floor8,
				("elevator_8", Phrase_Floor8)
			},
			{
				ElevatorFloorNumber.Floor1,
				("elevator_1", Phrase_Floor1)
			},
			{
				ElevatorFloorNumber.Floor0,
				("elevator_0", Phrase_Lobby)
			},
			{
				ElevatorFloorNumber.FloorMinus1,
				("elevator_minus1", Phrase_Basement)
			}
		};
	}
}

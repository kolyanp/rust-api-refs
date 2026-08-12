using UnityEngine;

namespace Rust.Modular;

public class ItemModVehicleChassis : ItemMod, VehicleModuleInformationPanel.IVehicleModuleInfo
{
	public GameObjectRef entityPrefab;

	[Range(1f, 6f)]
	public int socketsTaken = 1;

	public static readonly Phrase CantMoveChassisError;

	public int SocketsTaken => socketsTaken;

	static ItemModVehicleChassis()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		CantMoveChassisError = new Phrase("error.chassismove", "Cannot move item: Can't move vehicle chassis!");
	}
}

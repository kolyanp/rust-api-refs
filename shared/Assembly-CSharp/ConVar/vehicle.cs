using System;
using System.Runtime.CompilerServices;
using Facepunch;
using UnityEngine;

namespace ConVar;

[Factory("vehicle")]
public class vehicle : ConsoleSystem
{
	[Help("how long until boat corpses despawn (excluding tugboat - use tugboat_corpse_seconds)")]
	[ServerVar]
	public static float boat_corpse_seconds = 300f;

	[ServerVar(Help = "(Generated) When enabled, wheel colliders are disabled on vehicles that have gone to sleep in the physics engine, reducing CPU overhead for parked vehicles")]
	public static bool disable_wheels_when_sleeping = true;

	[ServerVar(Help = "(Generated) Distance in metres from the world boundary at which a repulsion force starts pushing vehicles back inward; prevents vehicles from leaving the playable area")]
	public static float world_boundary_force_start_distance = 100f;

	[ServerVar(Help = "(Generated) Distance in metres from a deep-sea portal boundary at which the repulsion force activates for vehicles")]
	public static float deepseaportal_boundary_force_start_distance = 10f;

	[ServerVar(Help = "(Generated) Additional offset applied to the world boundary force zone, extending the buffer zone inside the boundary before the force ramps up")]
	public static float world_boundary_force_offset = 25f;

	[ServerVar(Help = "If true, trains always explode when destroyed, and hitting a barrier always destroys the train immediately. Default: false")]
	public static bool cinematictrains = false;

	[ServerVar(Help = "Determines whether trains stop automatically when there's no-one on them. Default: false")]
	public static bool trainskeeprunning = false;

	[ServerVar(Help = "Determines whether modular cars turn into wrecks when destroyed, or just immediately gib. Default: true")]
	public static bool carwrecks = true;

	[ServerVar(Help = "Determines whether vehicles drop storage items when destroyed. Default: true")]
	public static bool vehiclesdroploot = true;

	[ServerVar(Help = "Braking force used by vehicle.train_stop (default 50000, same as train engine force)")]
	public static float train_brake_force = 50000f;

	[ServerVar(Help = "Acceleration force used by vehicle.train_speed (default 50000, same as train engine force)")]
	public static float train_accel_force = 50000f;

	[ServerUserVar]
	public static void swapseats(Arg arg)
	{
		int seat = -1;
		TryMovePlayerToSeat(ArgEx.Player(arg), seat);
	}

	[ServerUserVar]
	public static void swaptoseat(Arg arg)
	{
		int seat = arg.GetInt(0, -1);
		TryMovePlayerToSeat(ArgEx.Player(arg), seat);
	}

	public static void TryMovePlayerToSeat(BasePlayer ply, int seat)
	{
		if ((Object)(object)ply == (Object)null || ply.SwapSeatCooldown())
		{
			return;
		}
		BaseMountable mounted = ply.GetMounted();
		if (!((Object)(object)mounted == (Object)null))
		{
			BaseVehicle baseVehicle = ((Component)mounted).GetComponent<BaseVehicle>();
			if ((Object)(object)baseVehicle == (Object)null)
			{
				baseVehicle = mounted.VehicleParent();
			}
			if (!((Object)(object)baseVehicle == (Object)null))
			{
				baseVehicle.SwapSeats(ply, seat);
			}
		}
	}

	[ServerVar(Help = "Fixes up vehicles within 10m of the player by repairing to full hp, adding fuel to engines, and more")]
	public static void fixcars(Arg arg)
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((Object)(object)basePlayer == (Object)null)
		{
			arg.ReplyWith("Null player.");
			return;
		}
		if (!basePlayer.IsAdmin)
		{
			arg.ReplyWith("Must be an admin to use fixcars.");
			return;
		}
		int num = arg.GetInt(0, 2);
		num = Mathf.Clamp(num, 1, 3);
		BaseVehicle[] array = BaseEntity.Util.FindAll<BaseVehicle>();
		int num2 = 0;
		BaseVehicle[] array2 = array;
		foreach (BaseVehicle baseVehicle in array2)
		{
			if (baseVehicle.isServer && Vector3.Distance(((Component)baseVehicle).transform.position, ((Component)basePlayer).transform.position) <= 10f && baseVehicle.AdminFixUp(num))
			{
				num2++;
			}
		}
		MLRS[] array3 = BaseEntity.Util.FindAll<MLRS>();
		foreach (MLRS mLRS in array3)
		{
			if (mLRS.isServer && Vector3.Distance(((Component)mLRS).transform.position, ((Component)basePlayer).transform.position) <= 10f && mLRS.AdminFixUp())
			{
				num2++;
			}
		}
		DiverPropulsionVehicle[] array4 = BaseEntity.Util.FindAll<DiverPropulsionVehicle>();
		foreach (DiverPropulsionVehicle diverPropulsionVehicle in array4)
		{
			if (diverPropulsionVehicle.isServer && Vector3.Distance(((Component)diverPropulsionVehicle).transform.position, ((Component)basePlayer).transform.position) <= 10f && diverPropulsionVehicle.AdminFixUp())
			{
				num2++;
			}
		}
		HotAirBalloon[] array5 = BaseEntity.Util.FindAll<HotAirBalloon>();
		foreach (HotAirBalloon hotAirBalloon in array5)
		{
			if (hotAirBalloon.isServer && Vector3.Distance(((Component)hotAirBalloon).transform.position, ((Component)basePlayer).transform.position) <= 10f && hotAirBalloon.AdminFixUp())
			{
				num2++;
			}
		}
		SmallEngine[] array6 = BaseEntity.Util.FindAll<SmallEngine>();
		foreach (SmallEngine smallEngine in array6)
		{
			if (smallEngine.isServer && Vector3.Distance(((Component)smallEngine).transform.position, ((Component)basePlayer).transform.position) <= 10f && smallEngine.AdminFixUp())
			{
				num2++;
			}
		}
		arg.ReplyWith($"Fixed up {num2} vehicles/engines.");
	}

	[ServerVar(Help = "(Generated) Toggles auto-hover mode on the mini-helicopter the calling player is piloting, maintaining altitude automatically without pilot input")]
	public static void autohover(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((Object)(object)basePlayer == (Object)null)
		{
			arg.ReplyWith("Null player.");
			return;
		}
		if (!basePlayer.IsAdmin)
		{
			arg.ReplyWith("Must be an admin to use autohover.");
			return;
		}
		BaseHelicopter baseHelicopter = basePlayer.GetMountedVehicle() as BaseHelicopter;
		if ((Object)(object)baseHelicopter != (Object)null)
		{
			bool flag = baseHelicopter.ToggleAutoHover(basePlayer);
			arg.ReplyWith($"Toggled auto-hover to {flag}.");
		}
		else
		{
			arg.ReplyWith("Must be mounted in a helicopter first.");
		}
	}

	[ServerVar(Help = "(Generated) Immediately stops all train entities on the server, zeroing their speed; useful for clearing deadlocked train paths")]
	public static void stop_all_trains(Arg arg)
	{
		TrainEngine[] array = Object.FindObjectsByType<TrainEngine>((FindObjectsSortMode)0);
		for (int i = 0; i < array.Length; i++)
		{
			array[i].StopEngine();
		}
		arg.ReplyWith("All trains stopped.");
	}

	private static TrainCar GetLookedAtTrain(BasePlayer ply)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		BaseNetworkable baseNetworkable = GamePhysics.TraceRealmEntity(GamePhysics.Realm.Server, ply.eyes.HeadRay(), 0f, 200f, 1218652417, (QueryTriggerInteraction)0);
		if (baseNetworkable is TrainCar result)
		{
			return result;
		}
		if ((Object)(object)baseNetworkable != (Object)null)
		{
			TrainCar trainCar = baseNetworkable.GetParentEntity() as TrainCar;
			if ((Object)(object)trainCar != (Object)null)
			{
				return trainCar;
			}
		}
		return null;
	}

	private static TrainCar FindTrainCar(Arg arg, int idArgIndex, out string error)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (BaseNetworkable.serverEntities.Find(ArgEx.GetEntityID(arg, idArgIndex)) is TrainCar { completeTrain: not null } trainCar)
		{
			error = null;
			return trainCar;
		}
		error = "Entity not found or is not a train car.";
		return null;
	}

	[ServerVar(Help = "Set a train's target speed in km/h. Usage: vehicle.train_speed <km/h> (look-at) or vehicle.train_speed <id> <km/h>. Negative = reverse.")]
	public static void train_speed(Arg arg)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		TrainCar trainCar;
		if (arg.HasArgs(2))
		{
			trainCar = FindTrainCar(arg, 0, out var error);
			if ((Object)(object)trainCar == (Object)null)
			{
				arg.ReplyWith(error);
				return;
			}
			float num = arg.GetFloat(1);
			float speed = num / 3.6f;
			trainCar.completeTrain.SetTrackSpeedCinematic(speed, train_accel_force);
			arg.ReplyWith($"Train {trainCar.net.ID} ({trainCar.ShortPrefabName}) target speed {num} km/h.");
			return;
		}
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((Object)(object)basePlayer == (Object)null)
		{
			arg.ReplyWith("Must be called from a player.");
			return;
		}
		trainCar = GetLookedAtTrain(basePlayer);
		if ((Object)(object)trainCar == (Object)null || trainCar.completeTrain == null)
		{
			arg.ReplyWith("Not looking at a train.");
			return;
		}
		float num2 = arg.GetFloat(0);
		float speed2 = num2 / 3.6f;
		trainCar.completeTrain.SetTrackSpeedCinematic(speed2, train_accel_force);
		arg.ReplyWith($"Train {trainCar.net.ID} ({trainCar.ShortPrefabName}) target speed {num2} km/h.");
	}

	[ServerVar(Help = "Set all trains to the same target speed in km/h. Usage: vehicle.train_speed_all <km/h>. Negative = reverse.")]
	public static void train_speed_all(Arg arg)
	{
		float num = arg.GetFloat(0);
		float speed = num / 3.6f;
		int num2 = 0;
		TrainEngine[] array = BaseEntity.Util.FindAll<TrainEngine>();
		foreach (TrainEngine trainEngine in array)
		{
			if (trainEngine.completeTrain != null)
			{
				trainEngine.completeTrain.SetTrackSpeedCinematic(speed, train_accel_force);
				num2++;
			}
		}
		arg.ReplyWith($"Set {num2} trains to {num} km/h.");
	}

	[ServerVar(Help = "Gradually brake a train to a stop. Usage: vehicle.train_stop (look-at) or vehicle.train_stop <id>.")]
	public static void train_stop(Arg arg)
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		TrainCar trainCar;
		if (arg.HasArgs())
		{
			trainCar = FindTrainCar(arg, 0, out var error);
			if ((Object)(object)trainCar == (Object)null)
			{
				arg.ReplyWith(error);
				return;
			}
		}
		else
		{
			BasePlayer basePlayer = ArgEx.Player(arg);
			if ((Object)(object)basePlayer == (Object)null)
			{
				arg.ReplyWith("Must be called from a player.");
				return;
			}
			trainCar = GetLookedAtTrain(basePlayer);
			if ((Object)(object)trainCar == (Object)null || trainCar.completeTrain == null)
			{
				arg.ReplyWith("Not looking at a train.");
				return;
			}
		}
		trainCar.completeTrain.CinematicBrake(train_brake_force);
		arg.ReplyWith($"Train {trainCar.net.ID} ({trainCar.ShortPrefabName}) braking.");
	}

	[ServerVar(Help = "List all train engines with their entity IDs and current speeds.")]
	public static void train_list(Arg arg)
	{
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		TextTable val = Pool.Get<TextTable>();
		try
		{
			val.AddColumn("id");
			val.AddColumn("name");
			val.AddColumn("speed");
			val.AddColumn("position");
			TrainEngine[] array = BaseEntity.Util.FindAll<TrainEngine>();
			foreach (TrainEngine trainEngine in array)
			{
				if (trainEngine.IsValid())
				{
					float num = ((trainEngine.completeTrain != null) ? trainEngine.completeTrain.GetTrackSpeedFor(trainEngine) : 0f);
					val.AddRow(new string[4]
					{
						((object)Unsafe.As<NetworkableId, NetworkableId>(ref trainEngine.net.ID)/*cast due to constrained. prefix*/).ToString(),
						trainEngine.ShortPrefabName,
						$"{num:F1} m/s",
						((object)((Component)trainEngine).transform.position/*cast due to constrained. prefix*/).ToString()
					});
				}
			}
			arg.ReplyWith(((object)val).ToString());
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[ServerVar(Help = "(Generated) Destroys all modular car vehicles currently spawned on the server")]
	public static void killcars(Arg args)
	{
		ModularCar[] array = BaseEntity.Util.FindAll<ModularCar>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Kill();
		}
	}

	[ServerVar(Help = "(Generated) Destroys all push bike vehicles currently spawned on the server")]
	public static void killpushbikes(Arg args)
	{
		Bike[] array = BaseEntity.Util.FindAll<Bike>();
		foreach (Bike bike in array)
		{
			if (bike.poweredBy == Bike.PoweredBy.Human)
			{
				bike.Kill();
			}
		}
	}

	[ServerVar(Help = "(Generated) Destroys all motorbike vehicles currently spawned on the server")]
	public static void killmotorbikes(Arg args)
	{
		Bike[] array = BaseEntity.Util.FindAll<Bike>();
		foreach (Bike bike in array)
		{
			if (bike.poweredBy == Bike.PoweredBy.Fuel)
			{
				bike.Kill();
			}
		}
	}

	[ServerVar(Help = "(Generated) Destroys all minicopter vehicles currently spawned on the server")]
	public static void killminis(Arg args)
	{
		PlayerHelicopter[] array = BaseEntity.Util.FindAll<PlayerHelicopter>();
		foreach (PlayerHelicopter playerHelicopter in array)
		{
			if (((Object)playerHelicopter).name.ToLower().Contains("minicopter"))
			{
				playerHelicopter.Kill();
			}
		}
	}

	[ServerVar(Help = "(Generated) Destroys all scrap transport helicopter vehicles currently spawned on the server")]
	public static void killscraphelis(Arg args)
	{
		ScrapTransportHelicopter[] array = BaseEntity.Util.FindAll<ScrapTransportHelicopter>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Kill();
		}
	}

	[ServerVar(Help = "(Generated) Destroys all train vehicles currently spawned on the server")]
	public static void killtrains(Arg args)
	{
		TrainCar[] array = BaseEntity.Util.FindAll<TrainCar>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Kill();
		}
	}

	[ServerVar(Help = "(Generated) Destroys all boat vehicles (rowboats, RHIBs) currently spawned on the server")]
	public static void killboats(Arg args)
	{
		BaseBoat[] array = BaseEntity.Util.FindAll<BaseBoat>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Kill();
		}
	}

	[ServerVar(Help = "(Generated) Destroys all drone vehicles currently spawned on the server")]
	public static void killdrones(Arg args)
	{
		Drone[] array = BaseEntity.Util.FindAll<Drone>();
		foreach (Drone drone in array)
		{
			if (!(drone is DeliveryDrone))
			{
				drone.Kill();
			}
		}
	}

	[ServerVar(Help = "Print out boat drift status for all boats")]
	public static void boatdriftinfo(Arg args)
	{
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		TextTable val = Pool.Get<TextTable>();
		try
		{
			val.AddColumn("id");
			val.AddColumn("name");
			val.AddColumn("position");
			val.AddColumn("status");
			val.AddColumn("drift");
			BaseBoat[] array = BaseEntity.Util.FindAll<BaseBoat>();
			BaseBoat[] array2 = array;
			foreach (BaseBoat baseBoat in array2)
			{
				if (baseBoat.IsValid())
				{
					string text = (baseBoat.IsAlive() ? "alive" : "dead");
					string driftStatus = baseBoat.GetDriftStatus();
					val.AddRow(new string[5]
					{
						((object)Unsafe.As<NetworkableId, NetworkableId>(ref baseBoat.net.ID)/*cast due to constrained. prefix*/).ToString(),
						baseBoat.ShortPrefabName,
						((object)((Component)baseBoat).transform.position/*cast due to constrained. prefix*/).ToString(),
						text,
						driftStatus
					});
				}
			}
			if (array.Length == 0)
			{
				args.ReplyWith("No boats in world");
			}
			args.ReplyWith(((object)val).ToString());
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}
}

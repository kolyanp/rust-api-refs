using UnityEngine;

namespace ConVar;

[Factory("heli")]
public class PatrolHelicopter : ConsoleSystem
{
	private const string path = "assets/prefabs/npc/patrol helicopter/patrolhelicopter.prefab";

	[ServerVar(Help = "(Generated) How many minutes the patrol helicopter stays airborne before self-destructing; default is 30 minutes")]
	public static float lifetimeMinutes = 30f;

	[ServerVar(Help = "(Generated) Number of gun hardpoints active on the patrol helicopter; set to 0 to disable its guns without despawning it")]
	public static int guns = 1;

	[ServerVar(Help = "(Generated) Multiplier applied to all bullet damage dealt by the patrol helicopter; 1.0 = normal, 2.0 = double damage")]
	public static float bulletDamageScale = 1f;

	[ServerVar(Help = "(Generated) Cone angle in degrees of the patrol helicopter gun spread; higher values make the helicopter less accurate")]
	public static float bulletAccuracy = 2f;

	[ServerVar(Help = "(Generated) Spawns a patrol helicopter at the calling admin position, bypassing the normal random spawn logic; primarily used for testing")]
	public static void drop(Arg arg)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (Object.op_Implicit((Object)(object)basePlayer))
		{
			Debug.Log((object)("heli called to : " + ((object)((Component)basePlayer).transform.position/*cast due to constrained. prefix*/).ToString()));
			BaseEntity baseEntity = GameManager.server.CreateEntity("assets/prefabs/npc/patrol helicopter/patrolhelicopter.prefab");
			if (Object.op_Implicit((Object)(object)baseEntity))
			{
				((Component)baseEntity).GetComponent<PatrolHelicopterAI>().SetInitialDestination(((Component)basePlayer).transform.position + new Vector3(0f, 10f, 0f), 0f);
				baseEntity.Spawn();
			}
		}
	}

	[ServerVar(Help = "(Generated) Orders the active patrol helicopter to fly to the calling admin position and attack there")]
	public static void calltome(Arg arg)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (Object.op_Implicit((Object)(object)basePlayer))
		{
			Debug.Log((object)("heli called to : " + ((object)((Component)basePlayer).transform.position/*cast due to constrained. prefix*/).ToString()));
			BaseEntity baseEntity = GameManager.server.CreateEntity("assets/prefabs/npc/patrol helicopter/patrolhelicopter.prefab");
			if (Object.op_Implicit((Object)(object)baseEntity))
			{
				((Component)baseEntity).GetComponent<PatrolHelicopterAI>().SetInitialDestination(((Component)basePlayer).transform.position + new Vector3(0f, 10f, 0f));
				baseEntity.Spawn();
			}
		}
	}

	[ServerVar(Help = "(Generated) Spawns and sends a patrol helicopter to the specified player or position, using the same logic as calltome but targeting another player")]
	public static void call(Arg arg)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)ArgEx.Player(arg)))
		{
			Debug.Log((object)"Helicopter inbound");
			BaseEntity baseEntity = GameManager.server.CreateEntity("assets/prefabs/npc/patrol helicopter/patrolhelicopter.prefab");
			if (Object.op_Implicit((Object)(object)baseEntity))
			{
				baseEntity.Spawn();
			}
		}
	}

	[ServerVar(Help = "(Generated) Orders the active patrol helicopter to perform a strafing run on the calling admin current position")]
	public static void strafe(Arg arg)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (Object.op_Implicit((Object)(object)basePlayer))
		{
			PatrolHelicopterAI heliInstance = PatrolHelicopterAI.heliInstance;
			if ((Object)(object)heliInstance == (Object)null)
			{
				Debug.Log((object)"no heli instance");
				return;
			}
			heliInstance.strafe_target = basePlayer;
			heliInstance.interestZoneOrigin = ((Component)basePlayer).transform.position;
			heliInstance.ExitCurrentState();
			heliInstance.State_Strafe_Enter(basePlayer);
		}
	}

	[ServerVar(Help = "(Generated) Orders the active patrol helicopter to orbit around the calling admin current position")]
	public static void orbit(Arg arg)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (Object.op_Implicit((Object)(object)basePlayer))
		{
			PatrolHelicopterAI heliInstance = PatrolHelicopterAI.heliInstance;
			if ((Object)(object)heliInstance == (Object)null)
			{
				Debug.Log((object)"no heli instance");
				return;
			}
			heliInstance.interestZoneOrigin = ((Component)basePlayer).transform.position;
			heliInstance.ExitCurrentState();
			heliInstance.State_Orbit_Enter(70f);
		}
	}

	[ServerVar(Help = "(Generated) Orders the active patrol helicopter to orbit and strafe the calling admin position simultaneously")]
	public static void orbitstrafe(Arg arg)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (Object.op_Implicit((Object)(object)basePlayer))
		{
			PatrolHelicopterAI heliInstance = PatrolHelicopterAI.heliInstance;
			if ((Object)(object)heliInstance == (Object)null)
			{
				Debug.Log((object)"no heli instance");
				return;
			}
			heliInstance.strafe_target = basePlayer;
			heliInstance.interestZoneOrigin = ((Component)basePlayer).transform.position;
			heliInstance.ExitCurrentState();
			heliInstance.State_OrbitStrafe_Enter();
		}
	}

	[ServerVar(Help = "(Generated) Moves the active patrol helicopter to the calling admin current position without entering combat mode")]
	public static void move(Arg arg)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (Object.op_Implicit((Object)(object)basePlayer))
		{
			PatrolHelicopterAI heliInstance = PatrolHelicopterAI.heliInstance;
			if ((Object)(object)heliInstance == (Object)null)
			{
				Debug.Log((object)"no heli instance");
				return;
			}
			heliInstance.interestZoneOrigin = ((Component)basePlayer).transform.position;
			heliInstance.ExitCurrentState();
			heliInstance.State_Move_Enter(((Component)basePlayer).transform.position);
		}
	}

	[ServerVar(Help = "(Generated) Orders the active patrol helicopter to immediately flee to a random distant location and disengage")]
	public static void flee(Arg arg)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (Object.op_Implicit((Object)(object)basePlayer))
		{
			PatrolHelicopterAI heliInstance = PatrolHelicopterAI.heliInstance;
			if ((Object)(object)heliInstance == (Object)null)
			{
				Debug.Log((object)"no heli instance");
				return;
			}
			heliInstance.interestZoneOrigin = ((Component)basePlayer).transform.position;
			heliInstance.ExitCurrentState();
			heliInstance.State_Flee_Enter();
		}
	}

	[ServerVar(Help = "(Generated) Orders the active patrol helicopter to resume normal patrol mode, following its randomised waypoint path across the map")]
	public static void patrol(Arg arg)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (Object.op_Implicit((Object)(object)basePlayer))
		{
			PatrolHelicopterAI heliInstance = PatrolHelicopterAI.heliInstance;
			if ((Object)(object)heliInstance == (Object)null)
			{
				Debug.Log((object)"no heli instance");
				return;
			}
			heliInstance.interestZoneOrigin = ((Component)basePlayer).transform.position;
			heliInstance.ExitCurrentState();
			heliInstance.State_Patrol_Enter();
		}
	}

	[ServerVar(Help = "(Generated) Forces the active patrol helicopter to die immediately, triggering its death explosion and crash sequence")]
	public static void death(Arg arg)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (Object.op_Implicit((Object)(object)basePlayer))
		{
			PatrolHelicopterAI heliInstance = PatrolHelicopterAI.heliInstance;
			if ((Object)(object)heliInstance == (Object)null)
			{
				Debug.Log((object)"no heli instance");
				return;
			}
			heliInstance.interestZoneOrigin = ((Component)basePlayer).transform.position;
			heliInstance.ExitCurrentState();
			heliInstance.State_Death_Enter();
		}
	}

	[ServerVar(Help = "(Generated) Triggers the helicopter puzzle sequence (approach, puzzle activation, reward) for testing the helicopter monument puzzle")]
	public static void testpuzzle(Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (Object.op_Implicit((Object)(object)basePlayer))
		{
			_ = basePlayer.IsDeveloper;
		}
	}
}

using System.Collections.Generic;
using Facepunch;
using UnityEngine;

namespace ConVar;

public class Creative : ConsoleSystem
{
	private static bool _allUsers;

	[ServerVar(Saved = true, Help = "(Generated) Failsafe toggle that must be true before any alwaysOn commands work; prevents items from accidentally entering the always-on state outside of creative mode")]
	public static bool alwaysOnEnabled;

	[ReplicatedVar(Help = "Bypass the 30s repair cooldown when repairing objects", Saved = true)]
	public static bool freeRepair;

	[ReplicatedVar(Help = "Build blocks for free", Saved = true)]
	public static bool freeBuild;

	[ReplicatedVar(Help = "Bypasses all placement checks", Saved = true)]
	public static bool freePlacement;

	[ReplicatedVar(Help = "Bypasses bypassHoldToPlaceDuration when deploying items", Saved = true)]
	public static bool bypassHoldToPlaceDuration;

	[ReplicatedVar(Help = "Bypasses limits on IO length and points", Saved = true)]
	public static bool unlimitedIo;

	[ReplicatedVar(Help = "Apply creative mode to the entire server", Saved = true)]
	public static bool allUsers
	{
		get
		{
			return _allUsers;
		}
		set
		{
			_allUsers = value;
		}
	}

	[ServerVar(Help = "(Generated) Enables or disables creative mode for a specific player by name or Steam ID; creative mode removes resource costs and unlocks building freely")]
	public static void toggleCreativeModeUser(Arg arg)
	{
		BasePlayer player = ArgEx.GetPlayer(arg, 0);
		bool flag = arg.GetBool(1);
		if ((Object)(object)player == (Object)null)
		{
			arg.ReplyWith("Invalid player provided " + arg.GetString(0));
			return;
		}
		player.SetPlayerFlag(BasePlayer.PlayerFlags.CreativeMode, flag);
		player.Command("debug.setcreative_ui", flag || allUsers);
		arg.ReplyWith($"{player.displayName} creative mode: {flag}");
	}

	[ServerVar(Help = "(Generated) Sets the always-on state for all IAlwaysOn entities on the server (e.g. lights, switches); only works when alwaysOnEnabled is true and caller is in creative mode")]
	public static void toggleAlwaysOnAll(Arg arg)
	{
		if (!alwaysOnEnabled)
		{
			return;
		}
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((Object)(object)basePlayer == (Object)null || !basePlayer.IsInCreativeMode)
		{
			return;
		}
		bool alwaysOn = arg.GetBool(0);
		BaseCombatEntity[] array = BaseEntity.Util.FindAll<BaseCombatEntity>();
		foreach (BaseCombatEntity baseCombatEntity in array)
		{
			if (!baseCombatEntity.isClient && baseCombatEntity is IAlwaysOn alwaysOn2)
			{
				alwaysOn2.SetAlwaysOn(alwaysOn);
			}
		}
	}

	[ServerUserVar(ServerAdmin = true)]
	public static void toggleAlwaysOnRadius(Arg arg)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		if (!alwaysOnEnabled)
		{
			return;
		}
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((Object)(object)basePlayer == (Object)null || !basePlayer.IsInCreativeMode)
		{
			return;
		}
		bool alwaysOn = arg.GetBool(0);
		float radius = Mathf.Clamp(arg.GetFloat(1), 0f, 100f);
		List<BaseCombatEntity> list = Pool.Get<List<BaseCombatEntity>>();
		global::Vis.Entities(((Component)basePlayer).transform.position, radius, list, -1, (QueryTriggerInteraction)2);
		foreach (BaseCombatEntity item in list)
		{
			if (!item.isClient && item is IAlwaysOn alwaysOn2)
			{
				alwaysOn2.SetAlwaysOn(alwaysOn);
			}
		}
		Pool.FreeUnmanaged<BaseCombatEntity>(ref list);
	}

	[ServerUserVar(ServerAdmin = true)]
	public static void toggleAlwaysOn(Arg arg)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		if (!alwaysOnEnabled)
		{
			return;
		}
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!((Object)(object)basePlayer == (Object)null) && basePlayer.IsInCreativeMode)
		{
			bool alwaysOn = arg.GetBool(0);
			RaycastHit hit = default(RaycastHit);
			if (Physics.Raycast(basePlayer.eyes.position, basePlayer.eyes.HeadForward(), ref hit, 5f, 1218652417, (QueryTriggerInteraction)1) && RaycastHitEx.GetEntity(hit) is IAlwaysOn alwaysOn2)
			{
				alwaysOn2.SetAlwaysOn(alwaysOn);
			}
		}
	}
}
